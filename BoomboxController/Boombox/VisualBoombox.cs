using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;

namespace BoomboxController
{
    public class VisualBoombox : MonoBehaviour
    {
        private static VisualBoombox _instance;

        public Coroutine Start(IEnumerator routine)
        {
            if (_instance == null)
            {
                _instance = new GameObject("VisualBoombox").AddComponent<VisualBoombox>();
                UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)(object)_instance);
            }
            return ((MonoBehaviour)_instance).StartCoroutine(routine);
        }

        public IEnumerator GetTexture(string url, BoomboxItem boombox)
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Plugin.instance.Log(uwr.error);
                }
                else
                {
                    var texture = DownloadHandlerTexture.GetContent(uwr);
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "QuadBoombox";
                    cube.transform.localScale = new Vector3(0.8f, 0.38f, 0.001f);
                    Vector3 rot = cube.transform.localRotation.eulerAngles;
                    rot.Set(0f, -90f, 0f);
                    cube.transform.localRotation = Quaternion.Euler(rot);
                    cube.transform.position = new Vector3(boombox.transform.position.x - 0.179f, boombox.transform.position.y, boombox.transform.position.z);
                    cube.transform.parent = boombox.transform;
                    cube.GetComponent<BoxCollider>().enabled = false;
                    cube.GetComponent<MeshRenderer>().material = new Material(Shader.Find("HDRP/Lit"));
                    cube.GetComponent<MeshRenderer>().material.mainTexture = texture;
                }
            }
        }

        //private const int MATERIAL_OPAQUE = 0;
        //private const int MATERIAL_TRANSPARENT = 1;

        //private void SetMaterialTransparent(Material material, bool enabled)
        //{
        //    material.SetFloat("_SurfaceType", enabled ? MATERIAL_TRANSPARENT : MATERIAL_OPAQUE);
        //    material.SetFloat("_BlendMode", enabled ? MATERIAL_TRANSPARENT : MATERIAL_OPAQUE);
        //    material.SetShaderPassEnabled("SHADOWCASTER", !enabled);
        //    material.renderQueue = enabled ? 3000 : 2000;
        //    material.SetFloat("_DstBlend", enabled ? 10 : 0);
        //    material.SetFloat("_SrcBlend", enabled ? 5 : 1);
        //    material.SetFloat("_ZWrite", enabled ? 0 : 1);
        //}
    }
}
