﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.WindowsDevicePortal;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Experimental.XR;

namespace Microsoft.MixedReality.Toolkit.Utilities
{
    /// <summary>
    /// Utilities which return, or manipulate a <seealso cref="UnityEngine.Ray"/>.
    /// </summary>
    public static class RayUtilities
    {
        /// <summary>
        /// Gets the <seealso cref="UnityEngine.Ray"/> representing the position and direction of the user's head.
        /// </summary>
        /// <returns>The <seealso cref="UnityEngine.Ray"/> for the pointer</param>
        public static Ray GetHeadGazeRay()
        {
            return new Ray(CameraCache.Main.transform.position, CameraCache.Main.transform.forward);
        }

        /// <summary>
        /// Gets the <seealso cref="UnityEngine.Ray"/> representing the position and direction of the user's eyes.
        /// </summary>
        /// <param name="ray">The <seealso cref="UnityEngine.Ray"/> being returned</param>
        /// <returns>
        /// True if the ray is being returned, false otherwise.
        /// </returns>
        public static bool TryGetEyeGazeRay(out Ray ray)
        {
            ray = new Ray();

            IMixedRealityEyeGazeProvider eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;
            if (eyeGazeProvider == null) { return false; }

            ray.origin = eyeGazeProvider.GazeOrigin;
            ray.direction = eyeGazeProvider.GazeDirection;
            return true;
        }

        /// <summary>
        /// Gets the <seealso cref="UnityEngine.Ray"/> assosiciated with the user's hand.
        /// </summary>
        /// <param name="hand">The handedness of the hand</param>
        /// <param name="ray">The <seealso cref="UnityEngine.Ray"/> being returned</param>
        /// <returns>
        /// True if the ray is being returned, false otherwise.
        /// </returns>
        public static bool TryGetHandRay(Handedness hand, out Ray ray)
        {
            ray = new Ray();

            IMixedRealityController controller;
            if (TryGetControllerInstance(InputSourceType.Hand, hand, out controller))
            {
                MixedRealityInteractionMapping mapping;
                if (TryGetInteractionMapping(controller, DeviceInputType.SpatialPointer, out mapping))
                {
                    ray.origin = mapping.PositionData;
                    ray.direction = MathUtilities.GetDirection(mapping.RotationData);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the <seealso cref="UnityEngine.Ray"/> assosiciated with the motion controller.
        /// </summary>
        /// <param name="hand">The handedness of the motion controller</param>
        /// <param name="ray">The <seealso cref="UnityEngine.Ray"/> being returned</param>
        /// <returns>
        /// True if the ray is being returned, false otherwise.
        /// </returns>
        public static bool TryGetMotionControllerRay(Handedness hand, out Ray ray)
        {
            ray = new Ray();

            IMixedRealityController controller;
            if (TryGetControllerInstance(InputSourceType.Controller, hand, out controller))
            {
                MixedRealityInteractionMapping mapping;
                if (TryGetInteractionMapping(controller, DeviceInputType.SpatialPointer, out mapping))
                {
                    ray.origin = mapping.PositionData;
                    ray.direction = MathUtilities.GetDirection(mapping.RotationData);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the <seealso cref="IMixedRealityController"/> instance matching the specified source type and hand.
        /// </summary>
        /// <param name="sourceType">Type type of the input source</param>
        /// <param name="hand">The handedness of the controller</param>
        /// <param name="controller">The <seealso cref="IMixedRealityController"/> instance being returned</param>
        /// <returns>
        /// True if the controller instance is beeing returned, false otherwise. 
        /// </returns>
        private static bool TryGetControllerInstance(InputSourceType sourceType, Handedness hand, out IMixedRealityController controller)
        {
            controller = null;

            foreach (IMixedRealityController c in CoreServices.InputSystem.DetectedControllers)
            {
                if ((c.InputSource?.SourceType == sourceType) &&
                    (c.ControllerHandedness == hand))
                {
                    controller = c;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the <seealso cref="MixedRealityInteractionMapping"/> matching the <seealso cref="DeviceInputType"/> for
        /// the specified controller.
        /// </summary>
        /// <param name="controller">The <seealso cref="IMixedRealityController"/> instance</param>
        /// <param name="inputType">The type of device input</param>
        /// <param name="mapping">The <seealso cref="MixedRealityInteractionMapping"/> being returned</param>
        /// <returns>
        /// True if the interaction mapping is being returned, false otherwise. 
        /// </returns>
        private static bool TryGetInteractionMapping(IMixedRealityController controller, DeviceInputType inputType, out MixedRealityInteractionMapping mapping)
        {
            mapping = null;

            MixedRealityInteractionMapping[] mappings = controller.Interactions;
            for (int i = 0; i < mappings.Length; i++)
            {
                if (mappings[i].InputType == inputType)
                {
                    mapping = mappings[i];
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the<seealso cref="UnityEngine.Ray"/> assosiciated with the desired input source type
        /// and hand.
        /// </summary>
        /// <param name="sourceType">The type of input source</param>
        /// <param name="hand">The handedness of the input source</param>
        /// <param name="ray">The <seealso cref="UnityEngine.Ray"/> for the pointer</param>
        /// <returns>
        /// True if the ray is being returned, false otherwise.
        /// </returns>
        public static bool TryGetRay(InputSourceType sourceType, Handedness hand, out Ray ray)
        {
            bool success = false;

            switch (sourceType)
            {
                case InputSourceType.Head:
                    // The head does not have a handedness, so we ignore the hand parameter.
                    ray = GetHeadGazeRay();
                    success = true;
                    break;

                case InputSourceType.Eyes:
                    // The eyes do not have a handedness, so we ignore the hand parameter.
                    success = TryGetEyeGazeRay(out ray);
                    break;

                case InputSourceType.Hand:
                    success = TryGetHandRay(hand, out ray);
                    break;

                case InputSourceType.Controller:
                    success = TryGetMotionControllerRay(hand, out ray);
                    break;

                default:
                    Debug.Log($"It is not supported to get the ray for {sourceType} sources.");
                    ray = new Ray();
                    success = false;
                    break;
            }
            return success;
        }
    }
}
