using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace Thicket
{
    [DefaultExecutionOrder(-1)]
    class Orphanise : MonoBehaviour
    {
        public void Start()
        {   // removes the object this is applied to from it's parent object
            gameObject.transform.SetParent(null);
        }
    }
}
