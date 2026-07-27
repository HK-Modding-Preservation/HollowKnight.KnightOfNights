using System.Linq;
using ItemChanger;
using KnightOfNights.Scripts.SharedLib;

namespace KnightOfNights.IC;

internal class PlandoModule : AbstractModule<PlandoModule>
{
    protected override PlandoModule Self() => this;

    private void Add<T>()
        where T : AbstractModule<T>, new() => ItemChangerMod.Modules.GetOrAdd<T>();

    protected override void InitializeInternal()
    {
        Add<AppearFixModule>();
        Add<ArchivesHazardModule>();
        Add<BenchesModule>();
        Add<DreamGateControllerModule>();
        Add<FallenGuardianModule>();
        Add<GlitchRepairsModule>();
        Add<MushroomRollersModule>();
        Add<RevekSongModule>();
        Add<SceneDataModule>();
        Add<ShadelessModule>();
        Add<WarriorsNotesModule>();
    }
}
