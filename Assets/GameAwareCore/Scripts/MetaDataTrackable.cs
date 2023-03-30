using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameAware {
    public abstract class MetaDataTrackable : MonoBehaviour, IMetaDataTrackable {

        protected string objectKey = string.Empty;
        public string ObjectKey { get { return objectKey; } }

        [SerializeField]
        [HideInInspector]
        protected MetaDataFrameType frameType;
        public MetaDataFrameType FrameType { get { return frameType; } set { frameType = value; } }

        [SerializeField]
        [HideInInspector]
        protected bool persistAcrossScenes;
        public bool PersistAcrossScenes { get { return persistAcrossScenes; } set { persistAcrossScenes = value; } }

        [SerializeField]
        [HideInInspector]
        protected ScreenSpaceReference screenRectStyle;
        public ScreenSpaceReference ScreenRectStyle { get { return screenRectStyle; } set { screenRectStyle = value; } }

        private Camera screenSpaceCamera = null;
        public Camera ScreenSpaceCamera {
            set {
                screenSpaceCamera = value;
            }
            get {
                return screenSpaceCamera == null ? MetaDataTracker.Instance.ScreenSpaceCamera : screenSpaceCamera;
            }
        }

        private Collider col;
        private Collider2D col2d;
        private Renderer ren;


        protected virtual void Awake() {
            objectKey = System.Guid.NewGuid().ToString();
        }

        protected virtual void Start() {
            col = GetComponent<Collider>();
            ren = GetComponent<Renderer>();
            col2d = GetComponent<Collider2D>();
            if (screenRectStyle == ScreenSpaceReference.NotSet) {
                if (ren != null) {
                    screenRectStyle = ScreenSpaceReference.Renderer;
                }
                else if (col != null) {
                    screenRectStyle = ScreenSpaceReference.Collider;
                }
                else { 
                    screenRectStyle = ScreenSpaceReference.Transform;
                }
            }
            StartCoroutine(RegisterTrackable());
        }

        protected virtual IEnumerator RegisterTrackable() {
            MetaDataTracker.Instance.AddTrackableObject(this);
            yield break;
        }

        protected virtual void OnDestroy() {
            MetaDataTracker.Instance.RemoveTrackableObject(this);
        }

        public virtual JObject InbetweenData() {
            switch (this.screenRectStyle) {
                case ScreenSpaceReference.Transform:
                case ScreenSpaceReference.Collider:
                case ScreenSpaceReference.Renderer:
                    return new JObject {
                        { IMetaDataTrackable.SCREEN_RECT_KEY, ScreenRect().ToJObject() }
                    };

                case ScreenSpaceReference.None:
                    return new JObject();
                default:
                    Debug.LogWarningFormat("MetaDataTrackable has ScreenRectStyle:{0}", screenRectStyle);
                    return new JObject();
            }
        }

        public virtual JObject KeyFrameData() {
            switch (this.screenRectStyle) {
                case ScreenSpaceReference.Transform:
                case ScreenSpaceReference.Collider:
                case ScreenSpaceReference.Renderer:
                    return new JObject {
                        { IMetaDataTrackable.SCREEN_RECT_KEY, ScreenRect().ToJObject() }
                    };
                    
                case ScreenSpaceReference.None:
                    return new JObject();
                default:
                    Debug.LogWarningFormat("MetaDataTrackable has ScreenRectStyle:{0}", screenRectStyle);
                    return new JObject();
            }
        }

        public virtual DepthRect ScreenRect() {
            switch (this.screenRectStyle) {
                case ScreenSpaceReference.Transform:
                    var pos = ScreenSpaceHelper.WorldToViewerScreenPoint(ScreenSpaceCamera, transform.position);
                    return new DepthRect((int)pos.x, (int)pos.y, 0, 0, pos.z);
                case ScreenSpaceReference.Collider:
                    if (col2d != null) {
                       return ScreenSpaceHelper.WorldBoundsToViewerScreenRect(ScreenSpaceCamera, col2d);
                    }
                    else {
                        return ScreenSpaceHelper.WorldBoundsToViewerScreenRect(ScreenSpaceCamera, col);
                    }
                case ScreenSpaceReference.Renderer:
                    return  ScreenSpaceHelper.WorldBoundsToViewerScreenRect(ScreenSpaceCamera, ren);
                case ScreenSpaceReference.None:
                    return DepthRect.zero;
                default:
                    Debug.LogWarning("Unrecognized ScreenRect Style");
                    return DepthRect.zero;
            }
        }
    }

}