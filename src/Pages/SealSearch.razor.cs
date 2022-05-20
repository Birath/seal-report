using System.Net.Http.Json;
using System.Runtime.InteropServices;
using TitleReport.Components.ProfileOverview;
using TitleReport.Data;

namespace TitleReport.Pages
{
    public partial class SealSearch
    {
	    private readonly string[] _raidSealNames = 
			{
				"Last Wish",
				"Garden of Salvation",
				"Vow of the Disciple",
				"Deep Stone Crypt",
				"Vault of Glass",
				"Black Armory",
				"A Shadow Rises"
			};

		public async Task<UserInfoCard?> SearchGuardianAsync(BungieName userRequest)
		{
			var userResponse = await Http.PostAsJsonAsync($"{Constants.BungieApiEndpoint}/Destiny2/SearchDestinyPlayerByBungieName/-1/", userRequest);
			if (userResponse.IsSuccessStatusCode)
			{
				var data = await userResponse.Content.ReadFromJsonAsync<BungieApiResponse<UserInfoCard[]>>();
				if (data is null || data.Message != "Ok" || data.Response is null)
				{
					await Console.Error.WriteLineAsync($"Failed to fetch user data from Bungie for {_bungieName}");
					return null;
				}

				var user = data.Response.Length == 1 ? data.Response.First() : data.Response!.FirstOrDefault(user => user.membershipType == user.crossSaveOverride);

				return user;
			}
			else
			{
				await Console.Error.WriteLineAsync(userResponse.ReasonPhrase);
			}
			return null;
		}

		public async Task<DestinyProfile?> GetDestinyProfile(UserInfoCard user)
        {
			var uri = new Uri($"{Constants.BungieApiEndpoint}/Destiny2/{user.membershipType}/Profile/{user.membershipId:D}/?components={ComponentType.Records},{ComponentType.PresentationNodes},{ComponentType.Characters}");
			var response = await Http.GetFromJsonAsync<BungieApiResponse<DestinyProfile>>(uri);

			if (response is null || response.ErrorStatus != BungieErrorStatus.Success || response.Response is null)
			{
				Console.Error.WriteLine("Failed to read profile");
				return null;
			}
			return response.Response;
		}

		public async Task<ProfileOverviewData> GetUserProfileOverview(string userName, IEnumerable<CharacterComponent> characters, RecordsComponent recordsData)
        {
			var latestPlayed = characters.MaxBy(character => character.dateLastPlayed);

			var equippedSealDefinition = await DataBaseManager!.GetRecordByIndexAsync<RecordDefinition>(Constants.RecordStoreName, "hash", latestPlayed!.titleRecordHash);
			
			var equippedSealNode = await GetSealPresentationNodeByRecordHash(equippedSealDefinition.hash);

			return equippedSealNode is null
                ? new ProfileOverviewData(userName, latestPlayed!.emblemBackgroundPath, new Seal("ERROR", "ERROR", "ERROR", FilterProperty.None, "", Array.Empty<Triumph>()))
                : new ProfileOverviewData(userName, $"{Constants.BungieManifestEndpoint}{latestPlayed!.emblemBackgroundPath}", CreateSeal(recordsData, equippedSealNode, equippedSealDefinition, Array.Empty<Triumph>()));
        }

        public async Task<IEnumerable<Seal>> GetUserSeals(RecordsComponent recordsData)
		{
			if (DataBaseManager is null) return Enumerable.Empty<Seal>();

			var records = recordsData.records;

			var profileSeals = new List<Seal>();

			var sealPresentationNodes = await DataBaseManager.Where<PresentationNodeDefinition>(Constants.PresentationNodesStoreName, "nodeType", PresentationNodeType.Records);
			var sealRootNode = await DataBaseManager.GetRecordByIndexAsync<PresentationNodeDefinition>(Constants.PresentationNodesStoreName, "hash", recordsData.recordSealsRootNodeHash);
			foreach (var sealNode in sealPresentationNodes)
            {
                if (sealNode?.completionRecordHash is null) continue;

                var sealDefinition = await DataBaseManager.GetRecordByIndexAsync<RecordDefinition>(Constants.RecordStoreName, "hash", sealNode.completionRecordHash.Value);

                if (sealDefinition is null)
                {
                    Console.Error.WriteLine($"Could not find Seal with hash {sealNode.completionRecordHash} in Record store");
                    continue;
                }

                var triumphs = await GetSealTriumphs(sealNode, records);
                var seal = CreateSeal(recordsData, sealNode, sealDefinition, triumphs);

                profileSeals.Add(seal);
            }

            var sortedTitles = profileSeals.OrderByDescending(seal => seal.IsGildedCurrentSeason)
						.ThenByDescending(seal => seal.GildedCount)
						.ThenByDescending(seal => seal.ActiveProperties.HasFlag(FilterProperty.Complete))
						.ThenBy(seal => seal.ActiveProperties.HasFlag(FilterProperty.Legacy));

			return sortedTitles;
		}

        private Seal CreateSeal(RecordsComponent recordsData, PresentationNodeDefinition sealNode, RecordDefinition sealDefinition, IEnumerable<Triumph> triumphs)
        {
			var sealComponent = recordsData.records[sealNode.completionRecordHash!.Value];
            var complete = sealComponent.state.HasFlag(RecordState.CanEquipTitle);
            var isLegacy = !sealNode.parentNodeHashes.Contains(recordsData.recordSealsRootNodeHash);

            var sealProperties = FilterProperty.None;
            sealProperties.Add(complete ? FilterProperty.Complete : FilterProperty.Incomplete);
            sealProperties.Add(isLegacy ? FilterProperty.Legacy : FilterProperty.Current);

            if (_raidSealNames.Contains(sealNode.displayProperties.name))
            {
                sealProperties.Add(FilterProperty.Raid);
            }
            var isGilded = false;
            var gildedCount = 0;
            if (sealDefinition.titleInfo.gildingTrackingRecordHash is { } gildingHash)
            {
                if (recordsData.records.TryGetValue(gildingHash, out var gildingTracking))
                {
                    isGilded = !gildingTracking.state.HasFlag(RecordState.ObjectiveNotCompleted);
                    gildedCount = gildingTracking.completedCount ?? 0;
                    if (gildedCount > 0)
                    {
                        sealProperties.Add(FilterProperty.Gilded);
                    }
                    else
                    {
                        sealProperties.Add(FilterProperty.Gildable);
                    }
                }
            }

            var seal = new Seal(
                sealDefinition.titleInfo.titlesByGender["Male"],
                sealNode.displayProperties.name,
                sealNode.displayProperties.description,
                sealProperties,
                $"{Constants.BungieManifestEndpoint}{sealNode.originalIcon}",
                triumphs
            )
            {
                IsGildedCurrentSeason = isGilded,
                GildedCount = gildedCount,
            };
            return seal;
        }

        private async Task<IEnumerable<Triumph>> GetSealTriumphs(PresentationNodeDefinition seal, Dictionary<uint, RecordComponent> userRecords)
		{
			var triumphs = new List<Triumph>();

			foreach (var triumphNode in seal.children.records)
			{
				var triumph = await DataBaseManager!.GetRecordByIndexAsync<RecordDefinition>(Constants.RecordStoreName, "hash", triumphNode.recordHash);
				if (!userRecords.ContainsKey(triumphNode.recordHash))
				{
					Console.WriteLine($"Missing: {triumph.displayProperties.name}");
				}
				else
				{
					var triumphComponent = userRecords[triumphNode.recordHash];
					bool isComplete = triumphComponent.objectives.All(o => o.complete);
					float progress = 1.0f;
					if (triumphComponent.objectives.Any())
					{
						progress = (float)triumphComponent.objectives.Sum(o => (o.progress / (float)o.completionValue) ?? 1.0) / triumphComponent.objectives.Length;

					}
					triumphs.Add(new Triumph(triumph.displayProperties.name, isComplete, progress));
				}

			}
			return triumphs.OrderBy(t => t.IsComplete);
		}
	
		private async Task<PresentationNodeDefinition?> GetSealPresentationNodeByRecordHash(uint hash)
        {
			var sealPresentationNodes = await DataBaseManager!.Where<PresentationNodeDefinition>(Constants.PresentationNodesStoreName, "nodeType", PresentationNodeType.Records);
			return sealPresentationNodes.FirstOrDefault(seal => seal.completionRecordHash == hash);
		}
	}

	public class Seal
	{
		public string Title { get; }
		public string Name { get; }
		public string Description { get; }
		public string Icon { get; }

		public bool IsComplete => ActiveProperties.HasFlag(FilterProperty.Complete);

		public bool IsLegacy => ActiveProperties.HasFlag(FilterProperty.Legacy);

		public bool IsGildedCurrentSeason { get; set; }

		public int GildedCount { get; set; }

		public IEnumerable<Triumph> RequiredTriumphs { get; }

		public FilterProperty ActiveProperties { get; }

		public Seal(string title, string name, string description, FilterProperty properties, string icon, IEnumerable<Triumph> triumphs)
		{
			Title = title;
			Name = name;
			Description = description;
			ActiveProperties = properties;
			Icon = icon;
			RequiredTriumphs = triumphs;
		}

	}

	public class Triumph
	{
		public string Name { get; }

		public bool IsComplete { get; }

		public float CompletionPercentage { get; }

		public Triumph(string name, bool isComplete, float completion)
		{
			Name = name;
			IsComplete = isComplete;
			CompletionPercentage = completion;
		}
	}

    [Flags]
	public enum FilterProperty
	{
		None = 0,
		Complete = 1 ,
		Incomplete = 2,
		Legacy = 4,
        Current = 8,
		Gilded = 16,
		Gildable = 32,
		Raid = 64,
    }

	public static class FilterPropertiesExtensions
	{ 
		public static void Add(ref this FilterProperty properties, FilterProperty property) 
		{
			properties |= property;
		}

		public static void Remove(ref this FilterProperty properties, FilterProperty property)
        {
			properties &= ~property;
        }
	}

}
