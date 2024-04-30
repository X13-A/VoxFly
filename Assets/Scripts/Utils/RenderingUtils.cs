using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class RenderingUtils : MonoBehaviour
{
    public static IEnumerator ConvertRenderTextureToTexture3D(RenderTexture rt3D, Action<Texture3D> onCompleted)
    {
        if (rt3D.dimension != UnityEngine.Rendering.TextureDimension.Tex3D)
        {
            Debug.LogError("Provided RenderTexture is not a 3D volume.");
            yield break; // Exit the coroutine early if the dimension is incorrect
        }

        int width = rt3D.width;
        int height = rt3D.height;
        int depth = rt3D.volumeDepth;
        var byteSize = width * height * depth * 1; // 1 is the size of one pixel

        // Allocate a NativeArray in the temporary job memory which gets cleaned up automatically
        var voxelData = new NativeArray<byte>(byteSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        AsyncGPUReadbackRequest request = AsyncGPUReadback.RequestIntoNativeArray(ref voxelData, rt3D);

        // Wait for the readback to complete
        while (!request.done)
        {
            yield return null; // Wait until the next frame
        }

        if (request.hasError)
        {
            Debug.LogError("GPU readback error detected.");
            voxelData.Dispose();
            yield break;
        }

        // Create the Texture3D from readback data
        Texture3D outputTexture = new Texture3D(width, height, depth, TextureFormat.R8, false);
        outputTexture.filterMode = FilterMode.Point;
        outputTexture.anisoLevel = 0;
        outputTexture.SetPixelData(voxelData, 0);
        outputTexture.Apply(updateMipmaps: false);
        
        voxelData.Dispose(); // Clean up the native array

        onCompleted?.Invoke(outputTexture); // Call the completion callback with the created Texture3D
    }

}
