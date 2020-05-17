using System;
using Player;
using TMPro;
using UnityEngine;

namespace Game
{
    public class TextPopup : MonoBehaviour
    {
        public static TextPopup Create(Vector3 position, int amount, bool isCriticalHit = false, int multiplier = 1)
        {
            //static class to spawn a points pop-up
            if (amount != 0)
            {
                GameObject pointsPopupObject =
                    Instantiate(PlayerManager.Instance.pointsPopupPrefab, position, Quaternion.identity);

                TextPopup textPopup = pointsPopupObject.GetComponent<TextPopup>();
                textPopup.Setup(amount, isCriticalHit, multiplier);

                return textPopup;
            }

            return null;
        }

        public static TextPopup CreateTextPopup(Vector3 position, string message, Color color)
        {
            //static class to display text pop-up
            if (message != "")
            {
                GameObject pointsPopupObject =
                    Instantiate(PlayerManager.Instance.pointsPopupPrefab, position, Quaternion.identity);

                TextPopup textPopup = pointsPopupObject.GetComponent<TextPopup>();
                textPopup.MessageSetup(message, color);

                return textPopup;
            }

            return null;
        }

        private static int sortingOrder;

        private const float DISAPPEAR_TIMER_MAX = 1f;

        private TextMeshPro textMesh;
        private float dissapearTimer;
        private Color textColor;
        private Vector3 moveVector;

        private void Awake()
        {
            textMesh = transform.GetComponent<TextMeshPro>();
        }

        private void LateUpdate()
        {
            textMesh.transform.LookAt(PlayerManager.Instance.transform);
        }

        public void Setup(int amount, bool isCriticalHit, int multiplier)
        {
            //sets the color + text of the pop-up + starts the upwards motion
            if (isCriticalHit)
            {
                textMesh.color = Color.red;
                textMesh.SetText(multiplier + " x " + amount);
            }
            else
            {
                textMesh.color = Color.white;
                textMesh.SetText(multiplier + " x " + amount);
            }

            textColor = textMesh.color;
            dissapearTimer = DISAPPEAR_TIMER_MAX;
            moveVector = new Vector3(0.2f, 1) * 10f;

            textMesh.sortingOrder = ++sortingOrder;
        }

        public void MessageSetup(string message, Color color)
        {
            //sets the color, message and lifetime of a text pop-up
            textMesh.color = color;
            textMesh.SetText(message);
            textColor = textMesh.color;
            dissapearTimer = DISAPPEAR_TIMER_MAX;
            moveVector = new Vector3(0.2f, 1) * 10f;

            textMesh.sortingOrder = ++sortingOrder;
        }

        private void Update()
        {
            //slowly moves the pop-up upwards + scales down over time and dissapears
            transform.position += moveVector * Time.deltaTime;
            moveVector -= Time.deltaTime * 8f * moveVector;

            if (dissapearTimer > DISAPPEAR_TIMER_MAX / 2)
            {
                //First half of lifetime
                float increaseScaleAmount = 1f;
                transform.localScale += Time.deltaTime * increaseScaleAmount * Vector3.one;
            }
            else
            {
                //Second half of lifetime
                float decreaseScaleAmount = 1f;
                transform.localScale -= Time.deltaTime * decreaseScaleAmount * Vector3.one;
            }

            dissapearTimer -= Time.deltaTime;

            if (dissapearTimer < 0)
            {
                //Start disappearing
                float disappearSpeed = 3f;
                textColor.a -= disappearSpeed * Time.deltaTime;
                textMesh.color = textColor;

                if (textColor.a < 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}