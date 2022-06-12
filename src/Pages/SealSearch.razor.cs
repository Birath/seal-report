using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json;
using BungieSharper.Entities;
using BungieSharper.Entities.Destiny;
using BungieSharper.Entities.Destiny.Components.Records;
using BungieSharper.Entities.Destiny.Definitions.Presentation;
using BungieSharper.Entities.Destiny.Definitions.Records;
using BungieSharper.Entities.Destiny.Entities.Characters;
using BungieSharper.Entities.Destiny.Responses;
using BungieSharper.Entities.Exceptions;
using TitleReport.Components.ProfileOverview;
using UserInfoCard = BungieSharper.Entities.User.UserInfoCard;

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
				ApiResponse<UserInfoCard[]>? data;
				try
				{
					data = await userResponse.Content.ReadFromJsonAsync<ApiResponse<UserInfoCard[]>>();
				}
				catch (JsonException)
				{
					await Console.Error.WriteLineAsync($"Got invalid UserInfo response from Bungie API");
					return null;
				}
				
				if (data!.ErrorCode != PlatformErrorCodes.Success || data.Response is null)
				{
					await Console.Error.WriteLineAsync($"Failed to fetch user data from Bungie for {_bungieName}: {data.ErrorStatus} ({data.ErrorCode})");
					return null;
				}
				
				var user = data.Response.Length == 1 ? data.Response.First() : data.Response!.FirstOrDefault(user => user.MembershipType == user.CrossSaveOverride);
				return user;
			}
			await Console.Error.WriteLineAsync(userResponse.ReasonPhrase);
			return null;
		}

		public async Task<DestinyProfileResponse?> GetDestinyProfile(UserInfoCard user)
        {
			var uri = new Uri($"{Constants.BungieApiEndpoint}/Destiny2/{user.MembershipType}/Profile/{user.MembershipId:D}/?components={DestinyComponentType.Records:D},{DestinyComponentType.PresentationNodes:D},{DestinyComponentType.Characters:D}");
			try
			{
				var response = await Http.GetFromJsonAsync<ApiResponse<DestinyProfileResponse>>(uri);
				if (response is not null && response.ErrorCode == PlatformErrorCodes.Success &&
				    response.Response is not null) return response.Response;
				await Console.Error.WriteLineAsync("Failed to read profile");
				return null;
			}
			catch (HttpRequestException ex)
			{
				await Console.Out.WriteLineAsync("Failed to get Destiny profile");
				await Console.Out.WriteLineAsync(ex.Message);
				return null;
			}

		}

		public async Task<ProfileOverviewData> GetUserProfileOverview(string userName, IEnumerable<DestinyCharacterComponent> characters, DestinyProfileRecordsComponent recordsData)
        {
			var latestPlayed = characters.MaxBy(character => character.DateLastPlayed);

			var equippedSealDefinition = await DataBaseManager!.GetRecordByIndexAsync<DestinyRecordDefinition>(Constants.RecordStoreName, "hash", latestPlayed!.TitleRecordHash);
			
			var equippedSealNode = await GetSealPresentationNodeByRecordHash(equippedSealDefinition.Hash);

			return equippedSealNode is null
                ? new ProfileOverviewData(userName, latestPlayed!.EmblemBackgroundPath, new Seal("ERROR", "ERROR", "ERROR", FilterProperty.None, "", Array.Empty<Triumph>()))
                : new ProfileOverviewData(userName, $"{Constants.BungieManifestEndpoint}{latestPlayed!.EmblemBackgroundPath}", CreateSeal(recordsData, equippedSealNode, equippedSealDefinition, Array.Empty<Triumph>()));
        }

        public async Task<IEnumerable<Seal>> GetUserSeals(DestinyProfileRecordsComponent recordsData)
		{
			if (DataBaseManager is null) return Enumerable.Empty<Seal>();

			var records = recordsData.Records;

			var profileSeals = new List<Seal>();

			var sealPresentationNodes = await DataBaseManager.Where<DestinyPresentationNodeDefinition>(Constants.PresentationNodesStoreName, "nodeType", DestinyPresentationNodeType.Records);
			foreach (var sealNode in sealPresentationNodes)
            {
                if (sealNode?.CompletionRecordHash is null) continue;

                var sealDefinition = await DataBaseManager.GetRecordByIndexAsync<DestinyRecordDefinition>(Constants.RecordStoreName, "hash", sealNode.CompletionRecordHash.Value);

                if (sealDefinition is null)
                {
                    Console.Error.WriteLine($"Could not find Seal with hash {sealNode.CompletionRecordHash} in Record store");
                    continue;
                }

                var triumphs = await GetSealTriumphs(sealNode, records);
                var seal = CreateSeal(recordsData, sealNode, sealDefinition, triumphs);
                if (seal != null) profileSeals.Add(seal);
            }

            var sortedTitles = profileSeals.OrderByDescending(seal => seal.IsGildedCurrentSeason)
						.ThenByDescending(seal => seal.GildedCount)
						.ThenByDescending(seal => seal.ActiveProperties.HasFlag(FilterProperty.Complete))
						.ThenBy(seal => seal.ActiveProperties.HasFlag(FilterProperty.Legacy));

			return sortedTitles;
		}

        private Seal? CreateSeal(DestinyProfileRecordsComponent recordsData, DestinyPresentationNodeDefinition sealNode, DestinyRecordDefinition sealDefinition, IEnumerable<Triumph> triumphs)
        {
	        if (!recordsData.Records.TryGetValue(sealNode.CompletionRecordHash!.Value, out var sealComponent))
		        return null;
			
            var complete = sealComponent.State.HasFlag(DestinyRecordState.CanEquipTitle);
            var isLegacy = !sealNode.ParentNodeHashes.Contains(recordsData.RecordSealsRootNodeHash);

            var sealProperties = FilterProperty.None;
            sealProperties.Add(complete ? FilterProperty.Complete : FilterProperty.Incomplete);
            sealProperties.Add(isLegacy ? FilterProperty.Legacy : FilterProperty.Current);

            if (_raidSealNames.Contains(sealNode.DisplayProperties.Name))
            {
                sealProperties.Add(FilterProperty.Raid);
            }
            var isGilded = false;
            var gildedCount = 0;
            if (sealDefinition.TitleInfo.GildingTrackingRecordHash is { } gildingHash)
            {
                if (recordsData.Records.TryGetValue(gildingHash, out var gildingTracking))
                {
                    isGilded = !gildingTracking.State.HasFlag(DestinyRecordState.ObjectiveNotCompleted);
                    gildedCount = gildingTracking.CompletedCount ?? 0;
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
                sealDefinition.TitleInfo.TitlesByGender[DestinyGender.Male],
                sealNode.DisplayProperties.Name,
                sealNode.DisplayProperties.Description,
                sealProperties,
                $"{Constants.BungieManifestEndpoint}{sealNode.OriginalIcon}",
                triumphs
            )
            {
                IsGildedCurrentSeason = isGilded,
                GildedCount = gildedCount,
            };
            return seal;
        }

        private async Task<IEnumerable<Triumph>> GetSealTriumphs(DestinyPresentationNodeDefinition seal, IReadOnlyDictionary<uint, DestinyRecordComponent> userRecords)
		{
			var triumphs = new List<Triumph>();

			foreach (var triumphNode in seal.Children.Records)
			{
				var triumph = await DataBaseManager!.GetRecordByIndexAsync<DestinyRecordDefinition>(Constants.RecordStoreName, "hash", triumphNode.RecordHash);
				
				if (!userRecords.ContainsKey(triumphNode.RecordHash))
				{
					Console.WriteLine($"Missing: {triumph.DisplayProperties.Name}");
				}
				else
				{
					var triumphComponent = userRecords[triumphNode.RecordHash];
					if (triumphComponent.Objectives is null)
					{
						Console.Out.Write(triumph.DisplayProperties.Name);
					}
					var isComplete = triumphComponent.Objectives?.All(o => o.Complete) ?? false;
					var progress = 0.0f;
					if (triumphComponent.Objectives?.Any() ?? false)
					{
						progress = (float)triumphComponent.Objectives.Average(o => (o.Progress / (float)o.CompletionValue) ?? 1.0);

					}
					triumphs.Add(new Triumph(triumph.DisplayProperties.Name, isComplete, progress));
				}

			}
			return triumphs.OrderBy(t => t.IsComplete);
		}
	
		private async Task<DestinyPresentationNodeDefinition?> GetSealPresentationNodeByRecordHash(uint hash)
        {
			var sealPresentationNodes = await DataBaseManager!.Where<DestinyPresentationNodeDefinition>(Constants.PresentationNodesStoreName, "nodeType", DestinyPresentationNodeType.Records);
			return sealPresentationNodes.FirstOrDefault(seal => seal.CompletionRecordHash == hash);
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
