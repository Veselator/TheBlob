using System;
using UnityEngine;

namespace TheBlob.Events
{
    // ─────────────────────────────────────────────
    //  Event definitions — pure data, no logic
    // ─────────────────────────────────────────────

    public enum HandSide { Left, Right }

    public readonly struct HandGrabStartEvent
    {
        public readonly HandSide Side;
        public readonly Vector2 WorldPosition;

        public HandGrabStartEvent(HandSide side, Vector2 worldPosition)
        {
            Side = side;
            WorldPosition = worldPosition;
        }
    }

    public readonly struct HandMoveEvent
    {
        public readonly HandSide Side;
        public readonly Vector2 WorldPosition;

        public HandMoveEvent(HandSide side, Vector2 worldPosition)
        {
            Side = side;
            WorldPosition = worldPosition;
        }
    }

    public readonly struct HandGrabEndEvent
    {
        public readonly HandSide Side;

        public HandGrabEndEvent(HandSide side)
        {
            Side = side;
        }
    }

    public readonly struct HandGrabbedSurfaceEvent
    {
        public readonly HandSide Side;
        public readonly Collider2D Surface;
        public readonly Vector2 ContactPoint;

        public HandGrabbedSurfaceEvent(HandSide side, Collider2D surface, Vector2 contactPoint)
        {
            Side = side;
            Surface = surface;
            ContactPoint = contactPoint;
        }
    }

    public readonly struct HandReleasedSurfaceEvent
    {
        public readonly HandSide Side;

        public HandReleasedSurfaceEvent(HandSide side)
        {
            Side = side;
        }
    }

    public readonly struct KnockbackEvent
    {
        public readonly float Duration;

        public KnockbackEvent(float duration)
        {
            Duration = duration;
        }
    }

    // ─────────────────────────────────────────────
    //  Lightweight event bus — static, type-safe
    // ─────────────────────────────────────────────

    public static class BlobEvents
    {
        // Input → HandController
        public static event Action<HandGrabStartEvent> OnHandGrabStart;
        public static event Action<HandMoveEvent> OnHandMove;
        public static event Action<HandGrabEndEvent> OnHandGrabEnd;

        // HandController → VisualHand / Audio / VFX
        public static event Action<HandGrabbedSurfaceEvent> OnHandGrabbedSurface;
        public static event Action<HandReleasedSurfaceEvent> OnHandReleasedSurface;

        // DamageHandler → HandController
        public static event Action<KnockbackEvent> OnKnockback;

        public static void FireHandGrabStart(HandGrabStartEvent e) => OnHandGrabStart?.Invoke(e);
        public static void FireHandMove(HandMoveEvent e) => OnHandMove?.Invoke(e);
        public static void FireHandGrabEnd(HandGrabEndEvent e) => OnHandGrabEnd?.Invoke(e);
        public static void FireHandGrabbedSurface(HandGrabbedSurfaceEvent e) => OnHandGrabbedSurface?.Invoke(e);
        public static void FireHandReleasedSurface(HandReleasedSurfaceEvent e) => OnHandReleasedSurface?.Invoke(e);
        public static void FireKnockback(KnockbackEvent e) => OnKnockback?.Invoke(e);

        /// <summary>
        /// Call on scene unload to prevent leaks.
        /// </summary>
        public static void ClearAll()
        {
            OnHandGrabStart = null;
            OnHandMove = null;
            OnHandGrabEnd = null;
            OnHandGrabbedSurface = null;
            OnHandReleasedSurface = null;
            OnKnockback = null;
        }
    }
}