using PurenailCore.CollectionUtil;
using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

internal static class CameraPriorities
{
    internal delegate void UpdateCamera(ref Vector3 pos);

    internal const float CAMERA_OFFSETTER = 0f;
    internal const float BINOCULARS = 1f;

    private static readonly SortedMultimap<float, UpdateCamera> overrides = [];

    internal static void AddOverride(float priority, UpdateCamera update) =>
        overrides.Add(priority, update);

    internal static void RemoveOverride(float priority, UpdateCamera update) =>
        overrides.Remove(priority, update);

    private static void OverrideLateUpdate(
        On.CameraController.orig_LateUpdate orig,
        CameraController self
    )
    {
        orig(self);

        Vector3 pos = self.transform.position;
        foreach (var v in overrides.AsDict.Values)
        foreach (var u in v)
            u(ref pos);

        self.transform.position = pos;
    }

    static CameraPriorities() => On.CameraController.LateUpdate += OverrideLateUpdate;
}
