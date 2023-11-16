using UnityEngine;
using UnityEditor;
//using Microsoft.MixedReality.Toolkit.Utilities;

#if MIXED_REALITY_OPENXR
using Microsoft.MixedReality.OpenXR;
#else
using QRTracking.WindowsMR;
#endif

namespace QRTracking
{
    public class SpatialGraphNodeTracker : MonoBehaviour
    {
        private System.Guid _id;
        private SpatialGraphNode node;

        public System.Guid Id
        {
            get => _id;

            set
            {
                if (_id != value)
                {
                    _id = value;
                    InitializeSpatialGraphNode(force: true);
                }
            }
        }

        // Use this for initialization
        void Start()
        {
            InitializeSpatialGraphNode();
        }

        // Update is called once per frame
        void Update()
        {
            InitializeSpatialGraphNode();

            if (node != null)
            {
                if (node.TryLocate(FrameTime.OnUpdate, out Pose pose))
                {
                 
                    if(Camera.main == null)
                    {
                        throw new System.Exception("No Camera has been tagged as 'MainCamera'");
                    }

                    if (Camera.main.transform.parent != null)
                    {
                        pose = pose.GetTransformedBy(Camera.main.transform.parent);
                    }
                    
                    gameObject.transform.SetPositionAndRotation(pose.position, pose.rotation);
                }
            }
        }

        private void InitializeSpatialGraphNode(bool force = false)
        {
            if (node == null || force)
            {
                node = (Id != System.Guid.Empty) ? SpatialGraphNode.FromStaticNodeId(Id) : null;
            }
        }
    }
}