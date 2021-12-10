using System;
using System.Linq;
using Possible.Vision.Managed;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Examples {
    /// <summary>
    /// This example demonstrates how to use the Vision and ARKit plugins together.
    /// </summary>
    public class ARKitExample2 : MonoBehaviour {
        public ARCameraManager cameraManager;
        public Vision vision;
        public Text uiText;

        private void Awake () {
            // We need to tell the Vision plugin what kind of requests do we want it to perform.
            // This call not only prepares the managed wrapper for the specified image requests,
            // but allocates VNRequest objects on the native side. You only need to call this
            // method when you initialize your app, and later if you need to change the type
            // of requests you want to perform. When performing image classification requests,
            // maxObservations refers to the number of returned guesses, ordered by confidence.
            vision.SetAndAllocateRequests (VisionRequest.Classification, maxObservations: 1);
        }

        private void OnEnable () {
            // Hook up to the completion event of object classification requests
            vision.OnObjectClassified += VisionOnObjectClassified;
            if (cameraManager != null) {
                cameraManager.frameReceived += OnCameraFrameReceived;
            }
        }

        private void OnDisable () {
            vision.OnObjectClassified -= VisionOnObjectClassified;
            if (cameraManager != null) {
                cameraManager.frameReceived -= OnCameraFrameReceived;
            }
        }

        unsafe void OnCameraFrameReceived (ARCameraFrameEventArgs eventArgs) {
            // We only classify a new image if no other vision requests are in progress
            if (vision.InProgress) {
                return;
            }

            // Attempt to get the latest camera image. If this method succeeds,
            // it acquires a native resource that must be disposed (see below).
            if (!cameraManager.TryAcquireLatestCpuImage (out XRCpuImage image)) {
                return;
            }

            XRCpuImage.ConversionParams conversionParams = new XRCpuImage.ConversionParams {
                // Get the entire image.
                inputRect = new RectInt (0, 0, image.width, image.height),

                // Downsample by 2.
                outputDimensions = new Vector2Int (image.width / 2, image.height / 2),

                // Choose RGBA format.
                outputFormat = TextureFormat.RGBA32,

                // Flip across the vertical axis (mirror image).
                transformation = XRCpuImage.Transformation.MirrorY
            };
            
            int size = image.GetConvertedDataSize(conversionParams);

            // Allocate a buffer to store the image.
            var buffer = new NativeArray<byte>(size, Allocator.Temp);
            var outputBuffer = new IntPtr (buffer.GetUnsafePtr ());
            
            try
            {
                image.Convert(conversionParams, outputBuffer, buffer.Length);
            }
            finally
            {
                // We must dispose of the XRCpuImage after we're finished
                // with it to avoid leaking native resources.
                image.Dispose();
            }
            
            vision.EvaluateBuffer (outputBuffer, ImageDataType.MetalTexture);
        }

        private void VisionOnObjectClassified (object sender, ClassificationResultArgs e) {
            // Display the top guess for the dominant object on the image
            uiText.text = e.observations.First ().identifier;
        }
    }
}