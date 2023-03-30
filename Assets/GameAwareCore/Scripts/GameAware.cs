using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace GameAware {
    public enum MetaDataFrameType {
        KeyFrame,
        Inbetween
    }

    public enum ScreenSpaceReference {
        NotSet,
		None,
        Transform,
        Collider,
        Renderer,
		Custom
    }

	public static class ScreenSpaceHelper {


		public static Vector3 ViewerScreenPointToWorldPoint(Vector2 screenPoint) {
            return ViewerScreenPointToWorldPoint(Camera.main, screenPoint, 0);
        }
        
		public static Vector3 ViewerScreenPointToWorldPoint(Vector2 screenPoint, float z) {
            return ViewerScreenPointToWorldPoint(Camera.main, screenPoint, z);
        }

        public static Vector3 ViewerScreenPointToWorldPoint(Camera camera, Vector2 screenPoint) {
			return ViewerScreenPointToWorldPoint(camera, screenPoint, 0);
        }

		/// <summary>
		/// Given a 2D Point in Viewer Screen Space return a Unity World Space Point at a given z depth from the given camera's perspective.
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="screenPoint"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public static Vector3 ViewerScreenPointToWorldPoint(Camera camera, Vector2 screenPoint, float z) {
			screenPoint.y = Screen.height - screenPoint.y;
			var worldPoint = camera.ScreenToWorldPoint(screenPoint);
			worldPoint.z = z;
			return worldPoint;
		}

		public static Vector3 WorldToViewerScreenPoint(Vector3 position) {
			return WorldToViewerScreenPoint(Camera.main, position);
		}

		public static Vector3 WorldToViewerScreenPoint(MonoBehaviour gameObject) {
			return WorldToViewerScreenPoint(Camera.main, gameObject.transform.position);
		}

		public static Vector3 WorldToViewerScreenPoint(Camera camera, MonoBehaviour gameObject) {
			return WorldToViewerScreenPoint(camera, gameObject.transform.position);
		}

		/// <summary>
		/// Given a Unity World Space point return a Viewer Screen Space Point based on the camera.
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public static Vector3 WorldToViewerScreenPoint(Camera camera, Vector3 position) {
			//camera.WorldToViewportPoint(position); <- this might work better because it can provide a z depth from camera
			//var screenPos = camera.WorldToScreenPoint(position);
			var viewPos = camera.WorldToViewportPoint(position);

			return new Vector3(camera.pixelWidth * viewPos.x,
                               camera.pixelHeight * (1 - viewPos.y),
							   viewPos.z);
		}

		private static Vector3[] boundsHelper = new Vector3[8];
        private static Vector3[] rectTransformHelper = new Vector3[4];
        private static Vector3 min = Vector3.zero;
		private static Vector3 max = Vector3.zero;

		public static DepthRect WorldBoundsToViewerScreenRect(Camera camera, Renderer renderer) {
			if (renderer == null) {
				Debug.LogWarning("ScreenRect Called on null Renderer");
				return DepthRect.zero;
			}
			return WorldBoundsToViewerScreenRect(camera, renderer.bounds);
		}

		public static DepthRect WorldBoundsToViewerScreenRect(Renderer renderer) {
			if (renderer == null) {
				Debug.LogWarning("ScreenRect Called on null Renderer");
				return DepthRect.zero;
			}
			return WorldBoundsToViewerScreenRect(Camera.main, renderer.bounds);
		}

		public static DepthRect WorldBoundsToViewerScreenRect(Camera camera, Collider collider) {
			if (collider == null) {
				Debug.LogWarning("ScreenRect Called on null Collider");
				return DepthRect.zero;
			}
			return WorldBoundsToViewerScreenRect(camera, collider.bounds);
		}

		public static DepthRect WorldBoundsToViewerScreenRect(Collider collider) {
			if (collider == null) {
				Debug.LogWarning("ScreenRect Called on null Collider");
				return DepthRect.zero;
			}
			return WorldBoundsToViewerScreenRect(Camera.main, collider.bounds);
		}

		public static DepthRect WorldBoundsToViewerScreenRect(Camera camera, Collider2D collider) {
			if(collider == null) {
                Debug.LogWarning("ScreenRect Called on null Collider");
                return DepthRect.zero;
            }
			return WorldBoundsToViewerScreenRect(camera, collider.bounds);
		}

        public static DepthRect WorldBoundsToViewerScreenRect(Collider2D collider) {
            if (collider == null) {
                Debug.LogWarning("ScreenRect Called on null Collider");
                return DepthRect.zero;
            }
			return WorldBoundsToViewerScreenRect(Camera.main, collider.bounds);

        }


        public static DepthRect WorldBoundsToViewerScreenRect(Bounds bounds) {
			return WorldBoundsToViewerScreenRect(Camera.main, bounds);
		}

		/// <summary>
		/// Given a bounds object return a Viewer Space rectangle from the perspective of the given camera.
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="bounds"></param>
		/// <returns></returns>
		public static DepthRect WorldBoundsToViewerScreenRect(Camera camera, Bounds bounds) {
			boundsHelper[0] = WorldToViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.max.y, bounds.min.z));  //ftl
			boundsHelper[1] = WorldToViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.max.y, bounds.min.z));  //ftr
			boundsHelper[2] = WorldToViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.min.y, bounds.min.z));  //fbr
			boundsHelper[3] = WorldToViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.min.y, bounds.min.z));  //fbl
			boundsHelper[4] = WorldToViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.max.y, bounds.max.z));  //btl
			boundsHelper[5] = WorldToViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.max.y, bounds.max.z));  //btr
			boundsHelper[6] = WorldToViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.min.y, bounds.max.z));  //bbr
			boundsHelper[7] = WorldToViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.min.y, bounds.max.z));  //bbl

			/* Get Max and Min bounding box positions */

			min = boundsHelper[0];
			max = boundsHelper[0];

			foreach (Vector3 vec in boundsHelper) {
				min = Vector3.Min(min, vec);
				max = Vector3.Max(max, vec);
			}

			//may need a custom rect struct that has x,y and ints and z as a float for depth
			return new DepthRect(min.x, min.y, max.x - min.x, max.y - min.y, min.z);
		}

        public static Vector2Int WorldBoundsToViewerScreenRectPosition(Camera camera, Renderer renderer) {
            if (renderer == null) {
                Debug.LogWarning("ScreenRect Called on null Renderer");
				return Vector2Int.zero;
            }
            return WorldBoundsToViewerScreenRectPosition(camera, renderer.bounds);
        }

        public static Vector2Int WorldBoundsToViewerScreenRectPosition(Renderer renderer) {
            if (renderer == null) {
                Debug.LogWarning("ScreenRect Called on null Renderer");
				return Vector2Int.zero;
            }
            return WorldBoundsToViewerScreenRectPosition(Camera.main, renderer.bounds);
        }

        public static Vector2Int WorldBoundsToViewerScreenRectPosition(Camera camera, Collider collider) {
            if (collider == null) {
                Debug.LogWarning("ScreenRect Called on null Collider");
				return Vector2Int.zero;
            }
            return WorldBoundsToViewerScreenRectPosition(camera, collider.bounds);
        }

        public static Vector2Int WorldBoundsToViewerScreenRectPosition(Collider collider) {
            if (collider == null) {
                Debug.LogWarning("ScreenRect Called on null Collider");
                return Vector2Int.zero;
            }
            return WorldBoundsToViewerScreenRectPosition(Camera.main, collider.bounds);
        }

		/// <summary>
		/// Given a bounds object return the position of the Viewer Space rectangle from the perspective of the given camera.
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="bounds"></param>
		/// <returns></returns>
        public static Vector2Int WorldBoundsToViewerScreenRectPosition(Camera camera, Bounds bounds) {
            boundsHelper[0] = WorldToViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.max.y, bounds.min.z));  //ftl
            boundsHelper[1] = WorldToViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.max.y, bounds.min.z));  //ftr
            boundsHelper[2] = WorldToViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.min.y, bounds.min.z));  //fbr
            boundsHelper[3] = WorldToViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.min.y, bounds.min.z));  //fbl
            boundsHelper[4] = WorldToViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.max.y, bounds.max.z));  //btl
            boundsHelper[5] = WorldToViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.max.y, bounds.max.z));  //btr
            boundsHelper[6] = WorldToViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.min.y, bounds.max.z));  //bbr
            boundsHelper[7] = WorldToViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.min.y, bounds.max.z));  //bbl

            /* Get Max and Min bounding box positions */

            min = boundsHelper[0];
            max = boundsHelper[0];

            foreach (Vector3 vec in boundsHelper) {
                min = Vector3.Min(min, vec);
                max = Vector3.Max(max, vec);
            }

			return new Vector2Int((int)min.x, (int)min.y);
        }

        public static DepthRect RectTransformToViewerScreenRect(RectTransform rectTransform) {
            return RectTransformToViewerScreenRect(Camera.main, rectTransform);
        }

        public static DepthRect RectTransformToViewerScreenRect(Camera camera, RectTransform rectTransform) {
            rectTransform.GetWorldCorners(rectTransformHelper);
            for (int i = 0; i < 4; i++) {
                // TODO - I'm not sure this approach is correct in all cases if a UI object is placed diegetically in worldspace.
                //Debug.LogFormat("<color=cyan>RectTransform</color> Pre WorldCorners[{0}] = {1}", i, rectTransformHelper[i]);
                //rectTransformHelper[i] = WorldToViewerScreenPoint(camera, rectTransformHelper[i]);
                rectTransformHelper[i].y = Screen.height - rectTransformHelper[i].y;
                //Debug.LogFormat("<color=cyan>RectTransform</color> Post WorldCorners[{0}] = {1}", i, rectTransformHelper[i]);
            }
            min = rectTransformHelper[0];
            max = rectTransformHelper[0];
            foreach (Vector3 vec in rectTransformHelper) {
                min = Vector3.Min(min, vec);
                max = Vector3.Max(max, vec);
            }
            return new DepthRect(min.x, min.y, max.x - min.x, max.y - min.y, min.z);
        }

    }

    public static class Constants {
        public const string SCREEN_RECT_KEY = "screenRect";
    }

    public static class Extensions {
		public static JObject ToJObject(this Vector2Int vec) {
			return new JObject {
				{"x", vec.x },
				{"y", vec.y }
			};
		}

		public static JObject ToJObject(this Vector2 vec) {
			return new JObject {
				{"x", vec.x },
				{"y", vec.y }
			};
		}

		public static JObject ToJObject(this Vector3 vec) {
			return new JObject {
				{"x",vec.x },
				{"y",vec.y },
				{"z",vec.z }
			};
        }

		public static JObject ToJObject(this RectInt rect) {
			return new JObject {
				{"x", rect.x },
				{"y", rect.y },
				{"w", rect.width },
				{"h", rect.height }
			};
		
		}

		public static JObject ToJObject(this MonoBehaviour mb) {
			JObject ret = new JObject();
			foreach (PropertyInfo pi in mb.GetType().GetProperties()){ 
				try {
					ret[pi.Name] = pi.GetValue(mb).ToString();
				}
				catch {
					continue;
				}
			}
			return ret;
        }

		public static JObject ToJObject(this GameObject gob) {
			JObject ret = new JObject();
			foreach (PropertyInfo pi in gob.GetType().GetProperties()) {
				try {
					ret[pi.Name] = pi.GetValue(gob).ToString();
				}
				catch {
					continue;
				}
			}
			JObject comps = new JObject();
			foreach(MonoBehaviour comp in gob.GetComponents<MonoBehaviour>()) {
				comps[comp.GetType().Name] = comp.ToJObject();
            }
			ret["Components"] = comps;
			return ret;
		}
	}

	public struct DepthRect {
		public RectInt rect;
		public float z;

        public int x { get { return rect.x; } }
        public int y { get { return rect.y; } }
        public int w { get { return rect.width; } }
        public int h { get { return rect.height; } }

        public static readonly DepthRect zero = new DepthRect(0, 0, 0, 0, 0);

		public DepthRect(int x, int y, int w, int h, float z) {
			this.rect = new RectInt(x, y, w, h);
			this.z = z;
		}

        public DepthRect(float x, float y, float w, float h, float z) {
			this.rect = new RectInt((int)x, (int)y, (int)w, (int)h);
            this.z = z;
        }

		public JObject ToJObject() {
            return new JObject {
                {"x", rect.x },
                {"y", rect.y },
                {"w", rect.width },
                {"h", rect.height },
                {"z", z },
            };
        }
	}
}
