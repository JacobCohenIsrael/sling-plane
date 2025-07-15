using UnityEngine;

namespace Gamefather.Game.Airplane
{
    [RequireComponent(typeof(Rigidbody))]
    public class Aerodynamics : MonoBehaviour
    {
        public enum PlaneUpgradeStage
        {
            FuselageOnly = 0,
            LeftWing = 1,
            RightWing = 2,
            Tail = 3,
            Propeller = 4
        }

        [Header("Upgrade Settings")]
        public PlaneUpgradeStage upgradeStage = PlaneUpgradeStage.FuselageOnly;
        public Transform leftWing;
        public Transform rightWing;
        public Transform tail;
        public Transform propeller;

        [Header("Lift Settings")]
        public float wingLiftCoefficient = 0.4f;
        public float baseDrag = 0.2f;

        [Header("Stability")]
        public float basePitchStability = 0.1f;
        public float tailStabilityBoost = 2f;

        [Header("Propeller")]
        public float propellerThrust = 5f;

        private Rigidbody rb;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            ApplyVisualUpgrades();
        }

        void FixedUpdate()
        {
            Vector3 velocity = rb.linearVelocity;
            if (velocity.magnitude < 0.1f) return;

            float totalDrag = baseDrag;
            float pitchStability = basePitchStability;

            // --- WING LIFT ---

            if (upgradeStage >= PlaneUpgradeStage.LeftWing && leftWing != null)
            {
                Vector3 lv = rb.GetPointVelocity(leftWing.position);
                Vector3 liftDir = Vector3.Cross(lv, transform.right).normalized;
                float lift = wingLiftCoefficient * lv.sqrMagnitude;
                rb.AddForceAtPosition(liftDir * lift, leftWing.position);

                totalDrag += 0.05f;
            }

            if (upgradeStage >= PlaneUpgradeStage.RightWing && rightWing != null)
            {
                Vector3 rv = rb.GetPointVelocity(rightWing.position);
                Vector3 liftDir = Vector3.Cross(rv, transform.right).normalized;
                float lift = wingLiftCoefficient * rv.sqrMagnitude;
                rb.AddForceAtPosition(liftDir * lift, rightWing.position);

                totalDrag += 0.05f;
            }

            // --- DRAG ---
            Vector3 dragForce = -velocity.normalized * velocity.sqrMagnitude * totalDrag;
            rb.AddForce(dragForce);

            // --- PROPULSION ---
            if (upgradeStage >= PlaneUpgradeStage.Propeller)
            {
                rb.AddForce(transform.forward * propellerThrust, ForceMode.Force);
            }

            // --- STABILITY ---
            if (upgradeStage >= PlaneUpgradeStage.Tail)
                pitchStability += tailStabilityBoost;

            Quaternion desiredRot = Quaternion.LookRotation(rb.linearVelocity.normalized, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, desiredRot, pitchStability * Time.fixedDeltaTime));
        }

        [ContextMenu("Upgrade")]
        public void Upgrade()
        {
            if (upgradeStage < PlaneUpgradeStage.Propeller)
            {
                upgradeStage++;
                ApplyVisualUpgrades();
            }
        }

        private void ApplyVisualUpgrades()
        {
            if (leftWing) leftWing.gameObject.SetActive(upgradeStage >= PlaneUpgradeStage.LeftWing);
            if (rightWing) rightWing.gameObject.SetActive(upgradeStage >= PlaneUpgradeStage.RightWing);
            if (tail) tail.gameObject.SetActive(upgradeStage >= PlaneUpgradeStage.Tail);
            if (propeller) propeller.gameObject.SetActive(upgradeStage >= PlaneUpgradeStage.Propeller);
        }
    }
}
