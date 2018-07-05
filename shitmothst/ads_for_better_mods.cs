using System.Collections;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace shitmothst
{
    public class ads_for_better_mods : MonoBehaviour
    {
        struct modAd
        {
            public string textDescription;
            public Color textColor;
        };

        private readonly modAd[] ads = new modAd[4]
        {
            new modAd()
            {
                textDescription = "Play Redwing.",
                textColor = new Color(1f, 0.78f, 0.2f)
            },
            new modAd()
            {
                textDescription = "Play Blackmoth.",
                textColor = new Color(0f, 0f, 0f, 1f)
            },
            new modAd()
            {
                textDescription = "Play Infinite Grimm.",
                textColor = new Color(0.93f, 0.53f, 0f)
            },
            new modAd()
            {
                textDescription = "Play Lightbringer.",
                textColor = Color.white
            }
        };
        
        
        private void Start()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += getRandomAd;
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= getRandomAd;
        }

        private void getRandomAd(Scene arg0, Scene arg1)
        {
            int randomAd = dash_hooks.rng.Next(0, ads.Length);
            Destroy(canvas);

            StartCoroutine(showAdAfterTime(randomAd));

        }


        private IEnumerator showAdAfterTime(int ad)
        {
            float timeToWait = (float) (dash_hooks.rng.NextDouble() * 20.0) + 1.0f;
            yield return new WaitForSeconds(timeToWait);
            makeModCanvas(ad);
        }


        private void makeModCanvas(int ad)
        {
            
            if (canvas != null) return;
            
            CanvasUtil.CreateFonts();
            canvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
            GameObject go =
                CanvasUtil.CreateTextPanel(canvas, "", 27, TextAnchor.UpperCenter,
                    new CanvasUtil.RectData(
                        new Vector2(0, 0),
                        new Vector2(0, 0),
                        new Vector2(0, 0),
                        generateNonAnnoyingLocation(),
                        new Vector2(0.5f, 0.5f)));
            
            
            textObj = go.GetComponent<Text>();
            textObj.color = ads[ad].textColor;
            textObj.font = CanvasUtil.TrajanBold;
            textObj.text = "";
            textObj.fontSize = 50;
            textObj.text = ads[ad].textDescription;
            textObj.CrossFadeAlpha(1f, 0f, false);
            
        }

        private static Vector2 generateNonAnnoyingLocation()
        {
            int side = dash_hooks.rng.Next(0, 4);
            double randomY;

            float xPos = 1.0f;
            
            // top
            if ( (side & 1) != 0 )
            {
                randomY = (dash_hooks.rng.NextDouble() * 0.2) + 0.75;
                xPos = 1.5f;
            }
            //bot
            else
            {
                randomY = (dash_hooks.rng.NextDouble() * 0.15) + 0.05;
            }
            
            return new Vector2(xPos, (float) (randomY));
        }
        
        
        private Text textObj;
        private GameObject canvas;
    }
}