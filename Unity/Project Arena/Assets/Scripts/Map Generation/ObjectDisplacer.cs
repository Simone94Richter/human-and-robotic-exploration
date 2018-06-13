using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ObjectDisplacer is a class used to place objects in a map.
/// </summary>
public class ObjectDisplacer : CoreComponent {

    // Tells if the expected height is negative.
    [SerializeField] private bool negativeHeight = false;
    // Height correction factor.
    [SerializeField] private float heightCorrection = 0f;
    // Object scale correction factor.
    [SerializeField] private float sizeCorrection = 1f;
    // Custom objects that will be added to the map.
    [SerializeField] private CustomObject[] customObjects;

    // Dictionary associating a char to a list of objects.
    private Dictionary<char, CustomObjectList> charObjectsDictionary;
    // Dictionary associating a categoty to a list of objects.
    private Dictionary<String, List<GameObject>> categoryObjectsDictionary;

    // Height direction correction factor.
    private float heightDirCorrection = 1f;

    private void Start() {
        InitializeAll();

        SetReady(true);
    }

    // Creates all the category objects and adds them to the dictionary. An object with no category 
    // is assigned to the default one. Creates and adds the prefabs lists to the 
    // charObjectsDictionary. Sets the height direction correction value.
    private void InitializeAll() {
        charObjectsDictionary = new Dictionary<char, CustomObjectList>();
        categoryObjectsDictionary = new Dictionary<String, List<GameObject>>();

        foreach (CustomObject c in customObjects) {
            if (transform.Find("Default") && (c.category == "" || c.category == null)) {
                GameObject childObject = new GameObject("Default");
                childObject.transform.parent = transform;
                childObject.transform.localPosition = Vector3.zero;
            } else if (transform.Find(c.category) == null) {
                GameObject childObject = new GameObject(c.category);
                childObject.transform.parent = transform;
                childObject.transform.localPosition = Vector3.zero;
            }

            if (!charObjectsDictionary.ContainsKey(c.objectChar)) {
                charObjectsDictionary.Add(c.objectChar, new CustomObjectList(c));
            } else {
                charObjectsDictionary[c.objectChar].AddObject(c);
            }
        }

        if (negativeHeight) {
            heightDirCorrection = -1f;
        } else {
            heightDirCorrection = 1f;
        }
    }

    // Displace the custom objects inside the map.
    public void DisplaceObjects(char[,] map, float squareSize, float height) {
        // categoryObjectsDictionary.Clear();
        // DestroyAllCustomObjects();

        for (int x = 0; x < map.GetLength(0); x++) {
            for (int y = 0; y < map.GetLength(1); y++) {
                if (charObjectsDictionary.ContainsKey(map[x, y])) {
                    CustomObject currentObject = charObjectsDictionary[map[x, y]].GetObject();

                    GameObject childObject = (GameObject)Instantiate(currentObject.prefab);
                    childObject.name = currentObject.prefab.name;
                    childObject.transform.parent = transform.Find(currentObject.category);
                    childObject.transform.localPosition = new Vector3(squareSize * x,
                        heightDirCorrection * (heightCorrection + height +
                        currentObject.heightCorrection), squareSize * y);
                    childObject.transform.localScale *= sizeCorrection;

                    if (categoryObjectsDictionary.ContainsKey(currentObject.category)) {
                        categoryObjectsDictionary[currentObject.category].Add(childObject);
                    } else {
                        categoryObjectsDictionary.Add(currentObject.category,
                            new List<GameObject>());
                        categoryObjectsDictionary[currentObject.category].Add(childObject);
                    }
                }
            }
        }
    }

    // Remove all the custom objects.
    private void DestroyAllCustomObjects() {
        foreach (Transform category in transform) {
            foreach (Transform child in category) {
                GameObject.Destroy(child.gameObject);
            }
        }
    }

    // Returns all the object which have the category passed as parameter.
    public List<GameObject> GetObjectsByCategory(String category) {
        try {
            return categoryObjectsDictionary[category];
        } catch (KeyNotFoundException) {
            ManageError(Error.HARD_ERROR, "Error while populating the map, no object of category " +
                category + " found in the dictionary.");
            return null;
        }
    }

    // Custom object. 
    [Serializable]
    private struct CustomObject {
        // Character which defines the object.
        public char objectChar;
        // Category of the object (optional).
        public string category;
        // Prefab of the object.
        public GameObject prefab;
        // Heigth correction factor.
        public float heightCorrection;
    }

    // List of custom objects which share the same char. 
    private class CustomObjectList {
        public List<CustomObject> objs;

        public CustomObjectList(CustomObject obj) {
            objs = new List<CustomObject> {
                obj
            };
        }

        public void AddObject(CustomObject obj) {
            objs.Add(obj);
        }

        public CustomObject GetObject() {
            int i = UnityEngine.Random.Range(0, objs.Count);
            return objs[i];
        }
    }

}