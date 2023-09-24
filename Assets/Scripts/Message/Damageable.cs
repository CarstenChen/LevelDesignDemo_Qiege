using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Gamekit3D.Message;
using UnityEngine.Serialization;

namespace Gamekit3D
{
    public partial class Damageable : MonoBehaviour
    {

        public int maxHitPoints;

        public float invulnerabiltyTime;

        [Range(0.0f, 360.0f)]
        public float hitAngle = 360.0f;
        [Range(0.0f, 360.0f)]
        [FormerlySerializedAs("hitForwardRoation")] //SHAME!
        public float hitForwardRotation = 360.0f;

        public bool isInvulnerable { get; set; }
        public float currentHitPoints { get; private set; }

        public UnityEvent OnDeath, OnReceiveDamage, OnHitWhileInvulnerable, OnBecomeInvulnerable, OnBecomeVulnerable, OnResetDamage,OnFinalDeath;

        [EnforceType(typeof(Message.IMessageReceiver))]
        public List<MonoBehaviour> onDamageMessageReceivers;

        protected float m_timeSinceLastHit = 0.0f;
        protected Collider m_Collider;

        System.Action schedule;

        void Start()
        {
            ResetDamage();
            m_Collider = GetComponent<Collider>();
        }

        void Update()
        {
            if (isInvulnerable)
            {
                if (m_timeSinceLastHit <= 0f) OnBecomeInvulnerable.Invoke();
                m_timeSinceLastHit += Time.deltaTime;
                if (m_timeSinceLastHit > invulnerabiltyTime)
                {
                    m_timeSinceLastHit = 0.0f;
                    isInvulnerable = false;
                    OnBecomeVulnerable.Invoke();
                }
            }
        }

        public void ResetDamage()
        {
            currentHitPoints = maxHitPoints;
            isInvulnerable = false;
            m_timeSinceLastHit = 0.0f;
            if(OnResetDamage!=null)
            OnResetDamage.Invoke();
        }

        public void CompleteDeath()
        {
            isInvulnerable = false;
            OnFinalDeath.Invoke();
        }
        public void SetColliderState(bool enabled)
        {
            m_Collider.enabled = enabled;
        }

        public void ApplyDamage(DamageMessage data)
        {
            if (currentHitPoints <= 0)
            {//ignore damage if already dead. TODO : may have to change that if we want to detect hit on death...
                return;
            }

            if (isInvulnerable)
            {
                OnHitWhileInvulnerable.Invoke();
                return;
            }

            Vector3 forward = transform.forward;
            forward = Quaternion.AngleAxis(hitForwardRotation, transform.up) * forward;

            //we project the direction to damager to the plane formed by the direction of damage
            Vector3 positionToDamager = data.damageSource - transform.position;
            positionToDamager -= transform.up * Vector3.Dot(transform.up, positionToDamager);

            if (Vector3.Angle(forward, positionToDamager) > hitAngle * 0.5f)
                return;

            //isInvulnerable = true;
            currentHitPoints -= data.damage;
            //Debug.Log(currentHitPoints);
            if (currentHitPoints <= 0)
                schedule += OnDeath.Invoke; //This avoid race condition when objects kill each other.
            else if (currentHitPoints > maxHitPoints)
                currentHitPoints = maxHitPoints;
            else
                OnReceiveDamage.Invoke();

            var messageType = currentHitPoints <= 0 ? MessageType.DEAD : MessageType.DAMAGED;

            for (var i = 0; i < onDamageMessageReceivers.Count; ++i)
            {
                var receiver = onDamageMessageReceivers[i] as IMessageReceiver;
                receiver.OnReceiveMessage(messageType, this, data);
            }

            Debug.Log(string.Format("{0}", currentHitPoints));
        }

        void LateUpdate()
        {
            if (schedule != null)
            {
                schedule();
                schedule = null;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 forward = transform.forward;
            forward = Quaternion.AngleAxis(hitForwardRotation, transform.up) * forward;

            if (Event.current.type == EventType.Repaint)
            {
                UnityEditor.Handles.color = Color.blue;
                UnityEditor.Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(forward), 1.0f,
                    EventType.Repaint);
            }


            UnityEditor.Handles.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
            forward = Quaternion.AngleAxis(-hitAngle * 0.5f, transform.up) * forward;
            UnityEditor.Handles.DrawSolidArc(transform.position, transform.up, forward, hitAngle, 1.0f);
        }
#endif
    }

}