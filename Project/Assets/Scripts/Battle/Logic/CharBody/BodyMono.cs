using Battle.Logic.AllManager;
using Battle.Logic.Media;
using cfg;
using UnityEngine;

public class BodyMono : MonoBehaviour
{
    public BodyL BodyL;

    public Vector3 passiveMoveVelocity;

    public Vector3 passiveMoveAcceleration;

    public Vector3 skillMoveVelocity;

    public float skillMoveRateZ;

    public int lastMoveFixXInt;

    private void OnCollisionEnter(Collision collision)
    {
        CheckHit(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        CheckHit(collision);
    }

    public void UpATick()
    {
        PassiveMove();
        SKillMove();
    }

    private void SKillMove()
    {
        if (skillMoveVelocity != Vector3.zero)
        {
            transform.Translate(skillMoveVelocity * BLogicMono.UpTickDeltaTimeSec * skillMoveRateZ);
        }
    }

    private void PassiveMove()
    {
        if (passiveMoveVelocity != Vector3.zero)
        {
            transform.position += passiveMoveVelocity * BLogicMono.UpTickDeltaTimeSec;

            var dot = Vector3.Dot(passiveMoveVelocity, passiveMoveAcceleration);
            if (dot < 0 && passiveMoveVelocity.magnitude < passiveMoveAcceleration.magnitude)
            {
                ResetPassiveMove();
            }
            else
            {
                passiveMoveVelocity += passiveMoveAcceleration * BLogicMono.UpTickDeltaTimeSec;
            }
        }
    }

    public void ResetPassiveMove()
    {
        passiveMoveVelocity = Vector3.zero;
        passiveMoveAcceleration = Vector3.zero;
    }

    public void ResetSkillMove()
    {
        skillMoveVelocity = Vector3.zero;
        skillMoveRateZ = 1f;
    }

    private void CheckHit(Collision collision)
    {
        var mediaMono = collision.collider.GetComponent<MediaMono>();
        var targetSectorFilter = mediaMono.TargetSectorFilter(transform.position);
        if (!targetSectorFilter)
        {
            return;
        }

        var mediaL = mediaMono.MediaL;
        BodyL.OnMediaHit(mediaL);
    }

    public void Release()
    {
        gameObject.SetActive(false);
    }

    public void SetSkillVelocity(MovementChangeCfg movementChangeCfg, int lastMoveTime)
    {
        var vector3 = lastMoveTime > 0 && lastMoveFixXInt != 0
            ? new Vector3((float)lastMoveFixXInt / lastMoveTime, 0, movementChangeCfg.ZInt / 1000f * skillMoveRateZ)
            : new Vector3(movementChangeCfg.XInt / 1000f, 0, movementChangeCfg.ZInt / 1000f * skillMoveRateZ);
        skillMoveVelocity = vector3;
    }
}