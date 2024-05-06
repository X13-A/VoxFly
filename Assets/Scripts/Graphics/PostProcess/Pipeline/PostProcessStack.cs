using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class PostProcessStack : MonoBehaviour
{
    [SerializeField] private List<PostProcessBase> processings = new List<PostProcessBase>();

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        EventManager.Instance.Raise(new StartPostProcessingEvent());
        if (processings.Count == 0)
        {
            Graphics.Blit(source, destination);
            return;
        }

        RenderTexture currentSource = source;
        RenderTexture temp = RenderTexture.GetTemporary(source.width, source.height);

        for (int i = 0; i < processings.Count; i++)
        {
            if (processings[i] == null) continue;
            processings[i].Apply(currentSource, temp);

            // Swap the buffers
            if (i < processings.Count - 1) // Avoid unnecessary copy on the last element
            {
                Graphics.Blit(temp, currentSource);
            }
        }

        Graphics.Blit(temp, destination);
        RenderTexture.ReleaseTemporary(temp);
    }
}
