using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitRagdoll : MonoBehaviour
{
    
    [SerializeField] private Transform ragdollRootBone;

    public void Setup(Transform originalRootBone)
    {
          MatchAllChildTransofrm(originalRootBone, ragdollRootBone);   	
          ApplyExplosionToRagdoll(ragdollRootBone, 300f, transform.position, 10f);
    }

    private void MatchAllChildTransofrm(Transform root, Transform clone)
    {
        foreach (Transform child in root)
        {
            Transform cloneChild = clone.Find(child.name);
            if( cloneChild != null)
            {
                cloneChild.position = child.position;
                cloneChild.rotation = child.rotation;

                MatchAllChildTransofrm(child, cloneChild); //Going throu all Bone in the Unit
            }
        }
    }

    private void ApplyExplosionToRagdoll(Transform root, float explosionForce, Vector3 explosionPosition, float explosionRange)
    {
        foreach (Transform child in root)
        {
            if(child.TryGetComponent<Rigidbody>(out Rigidbody childRigidbody))
            {
                childRigidbody.AddExplosionForce(explosionForce,explosionPosition,explosionRange);
                ApplyExplosionToRagdoll(child,explosionForce,explosionPosition,explosionRange);
            }
        }

    }

}