// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osuTK;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Tests.Visual.Beatmaps
{
    public class TestSceneBeatmapCard : OsuTestScene
    {
        private APIBeatmapSet[] testCases;

        #region Test case generation

        [BackgroundDependencyLoader]
        private void load()
        {
            var normal = CreateAPIBeatmapSet(Ruleset.Value);
            normal.HasVideo = true;
            normal.HasStoryboard = true;

            var undownloadable = getUndownloadableBeatmapSet();

            var someDifficulties = getManyDifficultiesBeatmapSet(11);
            someDifficulties.Title = someDifficulties.TitleUnicode = "some difficulties";
            someDifficulties.Status = BeatmapSetOnlineStatus.Qualified;

            var manyDifficulties = getManyDifficultiesBeatmapSet(100);
            manyDifficulties.Status = BeatmapSetOnlineStatus.Pending;

            var explicitMap = CreateAPIBeatmapSet(Ruleset.Value);
            explicitMap.HasExplicitContent = true;

            var featuredMap = CreateAPIBeatmapSet(Ruleset.Value);
            featuredMap.TrackId = 1;

            var explicitFeaturedMap = CreateAPIBeatmapSet(Ruleset.Value);
            explicitFeaturedMap.HasExplicitContent = true;
            explicitFeaturedMap.TrackId = 2;

            var longName = CreateAPIBeatmapSet(Ruleset.Value);
            longName.Title = longName.TitleUnicode = "this track has an incredibly and implausibly long title";
            longName.Artist = longName.ArtistUnicode = "and this artist! who would have thunk it. it's really such a long name.";
            longName.HasExplicitContent = true;
            longName.TrackId = 444;

            testCases = new[]
            {
                normal,
                undownloadable,
                someDifficulties,
                manyDifficulties,
                explicitMap,
                featuredMap,
                explicitFeaturedMap,
                longName
            };
        }

        private APIBeatmapSet getUndownloadableBeatmapSet() => new APIBeatmapSet
        {
            OnlineID = 123,
            Title = "undownloadable beatmap",
            Artist = "test",
            Source = "more tests",
            Author = new APIUser
            {
                Username = "BanchoBot",
                Id = 3,
            },
            Availability = new BeatmapSetOnlineAvailability
            {
                DownloadDisabled = true,
            },
            Preview = @"https://b.ppy.sh/preview/12345.mp3",
            PlayCount = 123,
            FavouriteCount = 456,
            BPM = 111,
            HasVideo = true,
            HasStoryboard = true,
            Covers = new BeatmapSetOnlineCovers(),
            Beatmaps = new[]
            {
                new APIBeatmap
                {
                    RulesetID = Ruleset.Value.OnlineID,
                    DifficultyName = "Test",
                    StarRating = 6.42,
                }
            }
        };

        private static APIBeatmapSet getManyDifficultiesBeatmapSet(int count)
        {
            var beatmaps = new List<APIBeatmap>();

            for (int i = 0; i < count; i++)
            {
                beatmaps.Add(new APIBeatmap
                {
                    RulesetID = i % 4,
                    StarRating = 2 + i % 4 * 2,
                });
            }

            return new APIBeatmapSet
            {
                OnlineID = 1,
                Title = "many difficulties beatmap",
                Artist = "test",
                Author = new APIUser
                {
                    Username = "BanchoBot",
                    Id = 3,
                },
                HasVideo = true,
                HasStoryboard = true,
                Covers = new BeatmapSetOnlineCovers(),
                Beatmaps = beatmaps.ToArray(),
            };
        }

        #endregion

        private Drawable createContent(OverlayColourScheme colourScheme, Func<APIBeatmapSet, Drawable> creationFunc)
        {
            var colourProvider = new OverlayColourProvider(colourScheme);

            return new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(OverlayColourProvider), colourProvider)
                },
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background5
                    },
                    new BasicScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Full,
                            Padding = new MarginPadding(10),
                            Spacing = new Vector2(10),
                            ChildrenEnumerable = testCases.Select(creationFunc)
                        }
                    }
                }
            };
        }

        private void createTestCase(Func<APIBeatmapSet, Drawable> creationFunc)
        {
            foreach (var scheme in Enum.GetValues(typeof(OverlayColourScheme)).Cast<OverlayColourScheme>())
                AddStep($"set {scheme} scheme", () => Child = createContent(scheme, creationFunc));
        }

        [Test]
        public void TestNormal() => createTestCase(beatmapSetInfo => new BeatmapCard(beatmapSetInfo));
    }
}
