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
            public int weight;
            public int value;
        }

        public QRData qrData;

        public Dictionary<int, QRData> items = new Dictionary<int, QRData>() {
            {1, new QRData { id = 1, name = "Laptop", weight = 70, value = 10000 }},
            {2, new QRData { id = 2, name = "Router", weight = 25, value = 50 }},
            {3, new QRData { id = 3, name = "Maus", weight = 12, value = 25 }},
            {4, new QRData { id = 4, name = "Block", weight = 6, value = 12 }},
            {5, new QRData { id = 5, name = "Stifte", weight = 20, value = 22 }},
            {6, new QRData { id = 6, name = "Kopfhörer", weight = 25, value = 50 }},
            {7, new QRData { id = 7, name = "Taschenrechner", weight = 10, value = 20 }},
            {8, new QRData { id = 8, name = "Redbull", weight =5, value = 30 }},
            {9, new QRData { id = 9, name = "USB-Stick", weight = 1, value = 5 }},
            {10, new QRData { id = 10, name = "Verteiler", weight = 10, value = 15 }},
            {11, new QRData { id = 11, name = "Handy", weight = 30, value = 100 }}
        };

        public QRItem(int id)
        {
            items.TryGetValue(id, out qrData);
        }
    }
}