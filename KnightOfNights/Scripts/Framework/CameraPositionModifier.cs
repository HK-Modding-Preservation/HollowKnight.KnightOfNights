using System.Collections.Generic;
using MonoMod.Cil;
using PurenailCore.CollectionUtil;
using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

public delegate void ModifyCameraPosition(ref Vector3 position);

public enum CameraModifierPhase
{
    // Modify the camera's destination before camera locks and look offsets are applied.
    TARGET_BEFORE_LOCK,

    // Modify the camera's final position directly.
    FINAL_POSITON,
}

/// <summary>
/// Central utility for modifying the camera's position invisibly from other controllers.
/// </summary>
public static class CameraPositionModifier
{
    private static readonly Dictionary<
        CameraModifierPhase,
        SortedMultimap<float, ModifyCameraPosition>
    > modifiers = new()
    {
        [CameraModifierPhase.TARGET_BEFORE_LOCK] = [],
        [CameraModifierPhase.FINAL_POSITON] = [],
    };

    private static Vector3 origCameraPos;
    private static bool AnyFinalModifiers
    {
        set
        {
            if (field == value)
                return;
            if (!field)
                origCameraPos = GameManager.instance.cameraCtrl.transform.position;
            else
                GameManager.instance.cameraCtrl.transform.position = origCameraPos;

            field = value;
        }
    }

    public static void AddModifier(
        CameraModifierPhase phase,
        float priority,
        ModifyCameraPosition modifier
    )
    {
        modifiers[phase].Add(priority, modifier);
        AnyFinalModifiers = !modifiers[CameraModifierPhase.FINAL_POSITON].IsEmpty;
    }

    public static void RemoveModifier(
        CameraModifierPhase phase,
        float priority,
        ModifyCameraPosition modifier
    )
    {
        modifiers[phase].Remove(priority, modifier);
        AnyFinalModifiers = !modifiers[CameraModifierPhase.FINAL_POSITON].IsEmpty;
    }

    private static bool ApplyModifiers(CameraModifierPhase phase, ref Vector3 pos)
    {
        bool modified = false;
        foreach (var set in modifiers[phase].AsDict.Values)
        {
            foreach (var modifier in set)
            {
                modifier(ref pos);
                modified = true;
            }
        }

        return modified;
    }

    private static void AfterLateUpdate(
        On.CameraController.orig_LateUpdate orig,
        CameraController self
    )
    {
        orig(self);

        var pos = self.transform.position;
        if (ApplyModifiers(CameraModifierPhase.FINAL_POSITON, ref pos))
            self.transform.position = pos;
    }

    private static void HookCameraControl(ILContext ctx)
    {
        ILCursor cursor = new(ctx);

        cursor.Goto(0);
        cursor.GotoNext(
            MoveType.Before,
            i => i.MatchStfld<CameraController>(nameof(CameraController.destination))
        );
        cursor.EmitDelegate(
            (Vector3 pos) =>
            {
                Vector3 updated = pos;
                return ApplyModifiers(CameraModifierPhase.TARGET_BEFORE_LOCK, ref updated)
                    ? updated
                    : pos;
            }
        );
    }

    private static bool loaded = false;

    public static void Load()
    {
        if (loaded)
            return;
        loaded = true;

        On.CameraController.LateUpdate += AfterLateUpdate;
        IL.CameraController.LateUpdate += HookCameraControl;
    }

    static CameraPositionModifier() => Load();
}
