using UnityEngine;

namespace WCG
{
    public class FixedRotationObject : MonoBehaviour
    {
        Vector3 m_OriginalEulerAngles;
        float m_YCood;

        public bool FixedYCoodinate = false;

        void Update()
        {
            Vector3 deltaEulerAngles = transform.eulerAngles - m_OriginalEulerAngles;
			transform.Rotate(-deltaEulerAngles);
            if (FixedYCoodinate)
            {
				transform.position.Set(transform.position.x, m_YCood, transform.position.z);
            }
        }

        public void ResetTransformState()
        {
            m_OriginalEulerAngles = transform.eulerAngles;
            m_YCood = transform.position.y;
        }
    }
}