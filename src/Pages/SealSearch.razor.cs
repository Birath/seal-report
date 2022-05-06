using System.Net.Http.Json;

using TitleReport.Data;

namespace TitleReport.Pages
{
    public partial class SealSearch
    {
		public readonly string[] RaidSealNames = 
			{
				"Last Wish",
				"Garden of Salvation",
				"Vow of the Disciple",
				"Deep Stone Crypt",
				"Vault of Glass",
			};

		public async Task<UserInfoCard?> SearchGuardianAsync(BungieName userRequest)
		{
			var userResponse = await Http.PostAsJsonAsync($"{Constants.BungieApiEndpoint}/Destiny2/SearchDestinyPlayerByBungieName/-1/", userRequest);
			if (userResponse.IsSuccessStatusCode)
			{
				var data = await userResponse.Content.ReadFromJsonAsync<BungieApiResponse<UserInfoCard[]>>();
				if (data is null || data.Message != "Ok" || data.Response is null)
				{
					Console.Error.WriteLine($"Failed to fetch user data from Bungie for {bungieName}");
					return null;
				}

				var user = data.Response.Length == 1 ? data.Response.First() : data.Response!.FirstOrDefault(user => user.membershipType == user.crossSaveOverride);
				return user;
			}
			else
			{
				Console.Error.WriteLine(userResponse.ReasonPhrase);
			}
			return null;
		}

		public async Task<IEnumerable<Seal>> GetDestinyData(UserInfoCard user)
		{
			if (_dataBaseManager is null) return Enumerable.Empty<Seal>();

			var uri = new Uri($"{Constants.BungieApiEndpoint}/Destiny2/{user.membershipType}/Profile/{user.membershipId:D}/?components={ComponentType.Records},{ComponentType.PresentationNodes}");
			var response = await Http.GetFromJsonAsync<BungieApiResponse<DestinyProfile>>(uri);

			if (response is null || response.ErrorStatus != BungieErrorStatus.Success || response.Response is null)
			{
				Console.Error.WriteLine("Failed to read profile");
				return Enumerable.Empty<Seal>();
			}

			var profile = response.Response;
			var recordsData = profile.profileRecords?.data;
			if (recordsData is null) return Enumerable.Empty<Seal>();

			var records = recordsData.records;

			if (records is null) return Enumerable.Empty<Seal>();

			var profileSeals = new List<Seal>();

			var sealPresentationNodes = await _dataBaseManager.Where<PresentationNodeDefinition>(Constants.PresentationNodesStoreName, "nodeType", PresentationNodeType.Records);
			var sealRootNode = await _dataBaseManager.GetRecordByIndexAsync<PresentationNodeDefinition>(Constants.PresentationNodesStoreName, "hash", recordsData.recordSealsRootNodeHash);
			foreach (var sealNode in sealPresentationNodes)
			{
				if (sealNode is null || sealNode.completionRecordHash is null) continue;

				var profileSeal = records[sealNode.completionRecordHash.Value];
				var sealDefinition = await _dataBaseManager.GetRecordByIndexAsync<RecordDefinition>(Constants.RecordStoreName, "hash", sealNode.completionRecordHash.Value);

				if (sealDefinition is null)
				{
					Console.Error.WriteLine($"Could not find Seal with hash {sealNode.completionRecordHash} in Record store");
					continue;
				}
				var triumphs = await GetSealTriumphs(sealNode, records);
				var complete = profileSeal.state.HasFlag(RecordState.CanEquipTitle);
				bool isLegacy = !sealNode.parentNodeHashes.Contains(recordsData.recordSealsRootNodeHash);

				var seal = new Seal(
					sealDefinition.titleInfo.titlesByGender["Male"],
					sealNode.displayProperties.name,
					sealNode.displayProperties.description,
					new Uri($"{Constants.BungieManifestEndpoint}{sealNode.originalIcon}"),
					triumphs
				);

				if (complete)
				{
					seal.FilterProperties.Add("complete");
				}
				if (isLegacy)
				{
					seal.FilterProperties.Add("legacy");
				}

				if (RaidSealNames.Contains(seal.Name))
                {
					seal.FilterProperties.Add("raid");
                }

				if (sealDefinition.titleInfo.gildingTrackingRecordHash is uint gildingHash)
				{
					if (records.TryGetValue(gildingHash, out var gildingTracking))
					{
						seal.IsGildedCurrentSeason = !gildingTracking.state.HasFlag(RecordState.ObjectiveNotCompleted);
						seal.GildedCount = gildingTracking.completedCount ?? 0;
						if (seal.GildedCount > 0)
                        {
							seal.FilterProperties.Add("gilded");
                        }
					}
				}
				profileSeals.Add(seal);
			}

			var sortedTitles = profileSeals.OrderByDescending(seal => seal.IsGildedCurrentSeason)
						.ThenByDescending(seal => seal.GildedCount)
						.ThenByDescending(seal => seal.FilterProperties.Contains("complete"))
						.ThenBy(seal => seal.FilterProperties.Contains("legacy"));

			return sortedTitles;
		}

		private async Task<IEnumerable<Triumph>> GetSealTriumphs(PresentationNodeDefinition seal, Dictionary<uint, RecordComponent> userRecords)
		{
			var triumphs = new List<Triumph>();

			foreach (var triumphNode in seal.children.records)
			{
				var triumph = await _dataBaseManager!.GetRecordByIndexAsync<RecordDefinition>(Constants.RecordStoreName, "hash", triumphNode.recordHash);
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
	}

	public class Seal
	{
		public string Title { get; }
		public string Name { get; }
		public string Description { get; }
		public Uri Icon { get; }

		public bool IsComplete => FilterProperties.Contains("complete");

		public bool IsLegacy => FilterProperties.Contains("legacy");

		public bool IsGildedCurrentSeason { get; set; }

		public int GildedCount { get; set; }

		public IEnumerable<Triumph> RequiredTriumphs { get; }

		public HashSet<string> FilterProperties { get; } = new HashSet<string>();

		public Seal(string title, string name, string description, Uri icon, IEnumerable<Triumph> triumphs)
		{
			Title = title;
			Name = name;
			Description = description;
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
}
