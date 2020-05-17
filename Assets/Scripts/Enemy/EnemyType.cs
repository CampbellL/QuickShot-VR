using Game;
using UnityEngine;

namespace Test.EnemyDamageType.Script
{
    public class EnemyType : MonoBehaviour
    {
        public Renderer body;
        public Renderer shield;
        public enum EnemyDamageType {Red, Blue, Any};    //THIS IS FOR THE ENEMIES DAMAGING TYPE
    
        public enum EnemyShieldType {Red, Blue, Any, None};    //THIS IS FOR THE ENEMIES SHIELD TYPE
    
    
        public EnemyDamageType enemyType;    //THIS IS THE ENEMIES DAMAGING TYPE
        public EnemyShieldType enemyShieldType;    //THIS IS THE ENEMIES SHIELD TYPE
    
    
        public Material bodyRedMaterialRef;        //Those are the references for the materials
        public Material bodyBlueMaterialRef;
        public Material bodyAnyMaterialRef;
        
        public Material shieldRedMaterialRef;        //Those are the references for the materials
        public Material shieldBlueMaterialRef;
        public Material shieldAnyMaterialRef;

        private Camera _mainCam;

        // Start is called before the first frame update
        private void Start()
        {
            _mainCam = Camera.main;
            enemyType = (EnemyDamageType)Random.Range(0, 3);    //SETTING BASIC RANDOM STATS

            float percentage = Random.Range(0, 1f);

            if (percentage <= GameSettings.Instance.enemyShieldChance)
            {
                enemyShieldType = (EnemyShieldType) Random.Range(0, 2); //SETTING BASIC RANDOM STATS

                switch (enemyShieldType)
                {
                    case EnemyShieldType.Red when enemyType == EnemyDamageType.Red:
                        enemyShieldType = EnemyShieldType.Blue;
                        break;
                    case EnemyShieldType.Blue when enemyType == EnemyDamageType.Blue:
                        enemyShieldType = EnemyShieldType.Red;
                        break;
                    default:
                        enemyShieldType = EnemyShieldType.Any;
                        break;
                }
            }
            else
            {
                enemyShieldType = EnemyShieldType.None;
                shield.gameObject.SetActive(false);
            }

            SetVisual();
        }

        public bool CheckShield(bool isBlue, Vector3 hitpoint)
        {
            switch (enemyShieldType)
            {
                case EnemyShieldType.None:
                    return true;
                case EnemyShieldType.Any:
                case EnemyShieldType.Blue when isBlue:
                    shield.gameObject.SetActive(false);
                    enemyShieldType = EnemyShieldType.None;
                    break;
                case EnemyShieldType.Red when !isBlue:
                    shield.gameObject.SetActive(false);
                    enemyShieldType = EnemyShieldType.None;
                    break;
                default:
                    //spawn wrong weapon symbol
                    hitpoint.z -= 1f;
                    var icon = Instantiate(GameSettings.Instance.notAllowedPrefab, hitpoint, Quaternion.identity);
                    icon.transform.LookAt(_mainCam.transform);
                    icon.transform.Rotate(0, 0, 45);
                    Destroy(icon, 1f);
                    break;
            }

            return false;
        }

        public bool CheckEnemy(bool isBlue, Vector3 hitpoint)
        {
            switch(enemyType)
            {
                case EnemyDamageType.Any:
                case EnemyDamageType.Blue when isBlue:
                case EnemyDamageType.Red when !isBlue:
                    return true;
                default:
                    //spawn wrong weapon symbol
                    hitpoint.z -= 1f;
                    var icon = Instantiate(GameSettings.Instance.notAllowedPrefab, hitpoint, Quaternion.identity);
                    icon.transform.LookAt(_mainCam.transform);
                    icon.transform.Rotate(0, 0, 45);
                    Destroy(icon, 1f);
                    break;
            }

            return false;
        }

        private void SetVisual()
        {
            switch (enemyType)
            {
                case EnemyDamageType.Red:
                    body.material = bodyRedMaterialRef;
                    break;
                case EnemyDamageType.Blue:
                    body.material = bodyBlueMaterialRef;
                    break;
                case EnemyDamageType.Any:
                    body.material = bodyAnyMaterialRef;
                    break;
            }

            switch (enemyShieldType)
            {
                case EnemyShieldType.Red:
                    shield.material = shieldRedMaterialRef;
                    break;
                case EnemyShieldType.Blue:
                    shield.material = shieldBlueMaterialRef;
                    break;
                case EnemyShieldType.Any:
                    shield.material = shieldAnyMaterialRef;
                    break;
                case EnemyShieldType.None:
                    shield.gameObject.SetActive(false);
                    break;
            }
            
            //Hide the enemy
            body.material.SetFloat("_Amount", 1);
            shield.gameObject.SetActive(false);
        }

        public void DisplayShield()
        {
            if(enemyShieldType != EnemyShieldType.None)
                shield.gameObject.SetActive(true);
        }
    }
}
