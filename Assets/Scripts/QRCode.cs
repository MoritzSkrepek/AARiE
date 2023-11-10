using System.Collections;

using System.Collections.Generic;
using UnityEngine;

namespace QRTracking
{
    [RequireComponent(typeof(QRTracking.SpatialGraphNodeTracker))]
    public class QRCode : MonoBehaviour
    {
        public Microsoft.MixedReality.QR.QRCode qrCode;
        
        public float PhysicalSize { get; private set; }
        public string CodeText { get; private set; }

        private GameObject qrCodeCube;
        private GameObject QRInfo;

        private TextMesh QRID;
        private TextMesh QRNodeID;
        private TextMesh QRText;
        private TextMesh QRVersion;
        private TextMesh QRTimeStamp;
        private TextMesh QRSize;
        
        private long lastTimeStamp = 0;

        // Use this for initialization
        void Start()
        {
            gameObject.SetActive(true);

            PhysicalSize = 0.1f;
            CodeText = "Dummy";
            if (qrCode == null)
            {
                throw new System.Exception("QR Code Empty");
            }

            qrCodeCube = gameObject.transform.Find("Cube").gameObject;
            QRInfo = gameObject.transform.Find("QRInfo").gameObject;
            
            QRID = QRInfo.transform.Find("QRID").GetComponent<TextMesh>();
            QRNodeID = QRInfo.transform.Find("QRNodeID").GetComponent<TextMesh>();
            QRText = QRInfo.transform.Find("QRText").GetComponent<TextMesh>();
            QRVersion = QRInfo.transform.Find("QRVersion").GetComponent<TextMesh>();
            QRTimeStamp = QRInfo.transform.Find("QRTimeStamp").GetComponent<TextMesh>();
            QRSize = QRInfo.transform.Find("QRSize").GetComponent<TextMesh>();

            PhysicalSize = qrCode.PhysicalSideLength;
            CodeText = qrCode.Data;


            QRID.text = "Id:" + qrCode.Id.ToString();
            QRNodeID.text = "NodeId:" + qrCode.SpatialGraphNodeId.ToString();
            QRText.text = CodeText;

            QRVersion.text = "Ver: " + qrCode.Version;
            QRSize.text = "Size:" + qrCode.PhysicalSideLength.ToString("F04") + "m";
            QRTimeStamp.text = "Time:" + qrCode.LastDetectedTime.ToString("MM/dd/yyyy HH:mm:ss.fff");
            QRTimeStamp.color = Color.yellow;
            Debug.Log("Id= " + qrCode.Id + "NodeId= " + qrCode.SpatialGraphNodeId + " PhysicalSize = " + PhysicalSize + " TimeStamp = " + qrCode.SystemRelativeLastDetectedTime.Ticks + " QRVersion = " + qrCode.Version + " QRData = " + CodeText);
        }

        void UpdatePropertiesDisplay()
        {
            // Update properties that change
            if (qrCode != null && lastTimeStamp != qrCode.SystemRelativeLastDetectedTime.Ticks)
            {
                QRSize.text = "Size:" + qrCode.PhysicalSideLength.ToString("F04") + "m";

                QRTimeStamp.text = "Time:" + qrCode.LastDetectedTime.ToString("MM/dd/yyyy HH:mm:ss.fff");
                QRTimeStamp.color = QRTimeStamp.color == Color.yellow ? Color.white : Color.yellow;
                PhysicalSize = qrCode.PhysicalSideLength;
                Debug.Log("Id= " + qrCode.Id + "NodeId= " + qrCode.SpatialGraphNodeId + " PhysicalSize = " + PhysicalSize + " TimeStamp = " + qrCode.SystemRelativeLastDetectedTime.Ticks + " Time = " + qrCode.LastDetectedTime.ToString("MM/dd/yyyy HH:mm:ss.fff"));
                
                qrCodeCube.transform.localPosition = new Vector3(PhysicalSize / 2.0f, PhysicalSize / 2.0f, 0.0f);
                qrCodeCube.transform.localScale = new Vector3(PhysicalSize, PhysicalSize, 0.005f);
                lastTimeStamp = qrCode.SystemRelativeLastDetectedTime.Ticks;
                QRInfo.transform.localScale = new Vector3(PhysicalSize / 0.2f, PhysicalSize / 0.2f, PhysicalSize / 0.2f);
                Debug.Log("QRCube Pos = " + qrCodeCube.transform.localPosition.ToString("F7") + " QRCube Scale = " + qrCodeCube.transform.localScale.ToString("F7") + " QRInfo Scale = " + QRInfo.transform.localScale.ToString("F7"));
            }
        }

        // Update is called once per frame
        void Update()
        {
            UpdatePropertiesDisplay();
        }
    }
}