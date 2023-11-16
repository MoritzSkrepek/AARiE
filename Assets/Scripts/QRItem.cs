using System.Collections.Generic;
using UnityEngine;

namespace QRTracking
{
    public class QRItem
    {
        public struct QRData
        {
            public int id;
            public string name;
            public Vector3 position;
            public int size;
            public Form form;
            public int value;
        }

        public QRData qrData;
        private Dictionary<int, QRData> items = new Dictionary<int, QRData>() { {1, new QRData { id = 1, name = "Federpenal", position = Vector3.zero, size = 3, form = Form.rectangle, value = 0 }},
                                                                {2, new QRData { id = 2, name = "vereinzelt stifte", position = Vector3.zero, size = 1, form = Form.square, value = 0 }},
                                                                {3, new QRData { id = 3, name = "Kopfhörer", position = Vector3.zero, size = 9, form = Form.rectangle, value = 0 }},
                                                                {4, new QRData { id = 4, name = "Taschenrechner", position = Vector3.zero, size = 6, form = Form.rectangle, value = 0 }},
                                                                {5, new QRData { id = 5, name = "Festplatte", position = Vector3.zero, size = 6, form = Form.rectangle, value = 0 }},
                                                                {6, new QRData { id = 6, name = "USB-Stick", position = Vector3.zero, size = 4, form = Form.square, value = 0 }},
                                                                {7, new QRData { id = 7, name = "(Strom)Verteiler", position = Vector3.zero, size = 4, form = Form.rectangle, value = 0 }},
                                                                {8, new QRData { id = 8, name = "Netzteil", position = Vector3.zero, size = 6, form = Form.rectangle, value = 0 }}
        };


        public QRItem(int id)
        {
            items.TryGetValue(id, out qrData);
        }
    }
    public enum Form
    {
        square,
        line,
        rectangle
    }
}