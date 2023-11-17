using System.Collections;

using System.Collections.Generic;
using UnityEngine;


namespace QRTracking
{

    [RequireComponent(typeof(QRTracking.SpatialGraphNodeTracker))]
    public class QRCode : MonoBehaviour
    {
        public Microsoft.MixedReality.QR.QRCode qrCode;

        public QRItem item;
        
        public float PhysicalSize { get; private set; }

        private GameObject qrCodeCube;
        private GameObject model;
        
        private long lastTimeStamp = 0;

        // Use this for initialization
        void Start()
        {
            PhysicalSize = 0.1f;
           
            if (qrCode == null)
            {
                throw new System.Exception("QR Code Empty");
            }

            qrCodeCube = gameObject.transform.Find("Cube").gameObject;

            PhysicalSize = qrCode.PhysicalSideLength;

            try
            {
                item = new QRItem(int.Parse(qrCode.Data));
                model = gameObject.transform.Find(item.qrData.id.ToString()).gameObject;
                model.SetActive(model != null);
            } catch (System.Exception e)
            {
                Debug.LogError("Error parsing QR Code data: " + e.Message);
            }
            
            Debug.Log("Object: " + model.name + " " + model.activeSelf + " " + item.qrData.name + " " + item.qrData.name);
        }

        void UpdatePropertiesDisplay()
        {
            // Update properties that change
            if (qrCode != null && lastTimeStamp != qrCode.SystemRelativeLastDetectedTime.Ticks)
            {
                PhysicalSize = qrCode.PhysicalSideLength;

                item.qrData.position = new Vector3(PhysicalSize / 2.0f, PhysicalSize / 2.0f, 0.0f);
                qrCodeCube.transform.localPosition = item.qrData.position;
                qrCodeCube.transform.localScale = new Vector3(PhysicalSize, PhysicalSize, 0.005f);

                model.transform.localPosition = item.qrData.position;
                model.transform.localScale = new Vector3(PhysicalSize, PhysicalSize, PhysicalSize);

                lastTimeStamp = qrCode.SystemRelativeLastDetectedTime.Ticks;
            }
        }

        // Update is called once per frame
        void Update()
        {
            UpdatePropertiesDisplay();
        }
    }
}