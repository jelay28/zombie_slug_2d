// -----------------------------------------------------------------------------
// ObjectPool.cs - Piscina de objetos genérica.
// Crea "tamano" instancias desactivadas al cargar la escena y las reutiliza con
// SetActive. Se usa para proyectiles (20) y para cada tipo de zombi.
// Si se agota, crea una más y lo avisa en consola (indica piscina mal dimensionada).
// -----------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace ZombieSlug
{
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int tamano = 20;

        private readonly List<GameObject> objetos = new List<GameObject>();

        public int Tamano { get { return objetos.Count; } }

        private void Awake()
        {
            for (int i = 0; i < tamano; i++) objetos.Add(Crear());
        }

        private GameObject Crear()
        {
            GameObject go = Instantiate(prefab, transform);
            go.SetActive(false);
            return go;
        }

        public GameObject Obtener(Vector3 posicion)
        {
            for (int i = 0; i < objetos.Count; i++)
            {
                if (!objetos[i].activeSelf) return Activar(objetos[i], posicion);
            }
            Debug.LogWarning("[ObjectPool] " + name + " agotada: se crea una instancia extra.");
            GameObject extra = Crear();
            objetos.Add(extra);
            return Activar(extra, posicion);
        }

        private static GameObject Activar(GameObject go, Vector3 posicion)
        {
            go.transform.position = posicion;
            go.SetActive(true);
            return go;
        }
    }
}
