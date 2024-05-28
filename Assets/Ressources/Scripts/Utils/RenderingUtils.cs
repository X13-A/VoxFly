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
    public static float Get2DNoise(float x, float z, Vector2 scale, Vector2 offset)
    {
        Vector2 adjustedScale = new Vector3(scale.x, scale.y) / 983.3546789f; // Pour �viter les valeurs enti�res qui sont toujours les m�mes avec Mathf.PerlinNoise
        return Mathf.PerlinNoise(offset.x + x * adjustedScale.x, offset.y + z * adjustedScale.y);
    }

    public static float Get3DNoise(float x, float y, float z, Vector3 scale, Vector3 offset)
    {
        Vector3 adjustedScale = new Vector3(scale.x, scale.y, scale.z) / 983.3546789f; // Pour �viter les valeurs enti�res qui sont toujours les m�mes avec Mathf.PerlinNoise
        float ab = Mathf.PerlinNoise(offset.x + x * adjustedScale.x, offset.y + y * adjustedScale.y);
        float bc = Mathf.PerlinNoise(offset.y + y * adjustedScale.y, offset.z + z * adjustedScale.z);
        float ac = Mathf.PerlinNoise(offset.x + x * adjustedScale.x, offset.z + z * adjustedScale.z);

        float ba = Mathf.PerlinNoise(offset.y + y * adjustedScale.y, offset.x + x * adjustedScale.x);
        float cb = Mathf.PerlinNoise(offset.z + z * adjustedScale.z, offset.y + y * adjustedScale.y);
        float ca = Mathf.PerlinNoise(offset.z + z * adjustedScale.z, offset.x + x * adjustedScale.x);

        float abc = ab + bc + ac + ba + cb + ca;
        return abc / 6f;
    }

    public static IEnumerator ConvertRenderTextureToTexture3D(RenderTexture rt3D, int texelSize, TextureFormat textureFormat, TextureWrapMode textureWrapMode, FilterMode filterMode, Action<Texture3D> onCompleted = null)
    {
        if (rt3D.dimension != UnityEngine.Rendering.TextureDimension.Tex3D)
        {
            Debug.LogError("Provided RenderTexture is not a 3D volume.");
            yield break; // Exit the coroutine early if the dimension is incorrect
        }

        int width = rt3D.width;
        int height = rt3D.height;
        int depth = rt3D.volumeDepth;
        int byteSize = width * height * depth * texelSize;

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
        Texture3D outputTexture = new Texture3D(width, height, depth, textureFormat, false);
        outputTexture.filterMode = filterMode;
        outputTexture.anisoLevel = 0;
        outputTexture.SetPixelData(voxelData, 0);
        outputTexture.Apply(updateMipmaps: false);
        outputTexture.wrapMode = textureWrapMode;

        voxelData.Dispose(); // Clean up the native array

        onCompleted?.Invoke(outputTexture); // Call the completion callback with the created Texture3D
    }

}
