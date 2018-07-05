using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GlobalEnums;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace shitmothst
{
    public class dash_hooks : MonoBehaviour
    {
        public static System.Random rng = new System.Random();
        private Vector2 dashDirection;
        public GameObject sharpShadow;
        public PlayMakerFSM sharpShadowFSM;
        private readonly float dashCooldownTime = HeroController.instance.DASH_COOLDOWN_CH;
        private float dashCooldown = 0f;
        private bool completedCoroutines;

        private dash_antics dashAntics;
        private bool hasSharpShadowCached;
        private bool autoDashing;
        private bool turboDashing;

        private int dashAnticNumber;

        private const int TOTAL_DASH_ANTICS = 7;

        private const double maxTurboTime = 12.0;


        
        private void OnDestroy()
        {
            ModHooks.Instance.DashPressedHook -= dashTapped;
            ModHooks.Instance.DashVectorHook -= doDashDirection;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= resetAutoDash;

        }

        private void Start()
        {
            dashAnticNumber = 0;
            privateFields = new Dictionary<string, FieldInfo>();
            privateMethods = new Dictionary<string, MethodInfo>();
            dashAntics = new dash_antics();
            autoDashing = false;

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += resetAutoDash;
            
            try
            {
                StartCoroutine(configureHero());
                ModHooks.Instance.DashPressedHook += dashTapped;
                ModHooks.Instance.DashVectorHook += doDashDirection;
            }
            catch (Exception e)
            {
                log("Error setting up hooks. Error: " + e);
            }
        }

        private void Update()
        {
            if (autoDashing)
            {
                HeroController.instance.cState.invulnerable = true;
            }
            
            
        }

        private void resetAutoDash(Scene arg0, Scene arg1)
        {
            autoDashing = false;
            turboDashing = false;
            dashAntics.resetAngelMemes();
            
        }

        private IEnumerator configureHero()
        {
            while (HeroController.instance == null)
                yield return new WaitForEndOfFrame();

            if (sharpShadow != null && sharpShadow.CompareTag("Sharp Shadow")) yield break;
            
            
            foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go == null || !go.CompareTag("Sharp Shadow")) continue;
                    
                sharpShadow = go;
                sharpShadowFSM = FSMUtility.LocateFSM(sharpShadow, "damages_enemy");
                log($@"Found Sharp Shadow: {sharpShadow} - {sharpShadowFSM}.");
            }

            dashAntics.voidKnight = HeroController.instance.spellControl.gameObject;

            completedCoroutines = true;
            log("Completed needed coroutines. Your game is loaded.");
        }
        
        
        private Vector2 doDashDirection(Vector2 orig)
        {
            if (!completedCoroutines)
            {
                return orig;
            }
            Vector2 ret = dashDirectionRegular();


            switch (dashAnticNumber)
            {
                case 0:
                    //log("diagonal dash");
                    ret = dashAntics.diagonalDash(ret);
                    break;
                case 1:
                    //log("inverse dash");
                    ret = dashAntics.inverseDash(ret);
                    break;
                case 2:
                    //log("wave dash");
                    ret = dashAntics.waveDash(ret);
                    break;
                case 3:
                    if (autoDashing) return ret;
                    
                    log("Turbo blackmoth");
                    autoDashing = true;
                    StartCoroutine(blackmothTurboDash());
                    return ret;
                case 4:
                    if (autoDashing) return new Vector2(0f, 40f);
                    
                    log("Descending dank");
                    autoDashing = true;
                    StartCoroutine(descendingDank());
                    
                    return new Vector2(0f, 40f);
                case 5:
                    //log("zero dash");
                    return Vector2.zero;
                case 6:
                    if (autoDashing) return dashAntics.logSpiral(ret);
                    
                    log("log spiral dash");

                    autoDashing = true;
                    StartCoroutine(logDashWait());
                    
                    return dashAntics.logSpiral(ret);
                    
                default:
                    log("ERROR, antic number " + dashAnticNumber + " does not exist.");
                    return ret;
            }
            
            
            return ret;
        }
        
        
        private Vector2 dashDirectionRegular()
        {
            Vector2 ret;
            float num = getDashLength();
            if (dashDirection.y <= 0.02 && dashDirection.y >= -0.02)
            {
                ret = num * dashDirection;
                
            } else if (dashDirection.x <= 0.02 && dashDirection.x >= -0.02)
            {
                ret = num * dashDirection;
            }
            else
            {
                ret = (num / Mathf.Sqrt(2)) * dashDirection;
            }
            
            return ret;
        }

        private IEnumerator setInvulnFalseAfterTime(float time)
        {
            yield return new WaitForSeconds(time);
            
            HeroController.instance.cState.invulnerable = false;
        }

        private IEnumerator logDashWait()
        {
            float time = 0f;

            while (time < 3.0f && autoDashing)
            {
                time += Time.deltaTime;
                if (HeroController.instance.cState.dashing == false)
                    dashConditionChecker(true);
                yield return null;
            }

            while (HeroController.instance.cState.dashing && autoDashing)
            {
                yield return null;
            }
            autoDashing = false;

            StartCoroutine(setInvulnFalseAfterTime(0.5f));
        }

        private IEnumerator descendingDank()
        {
            while (HeroController.instance.cState.dashing)
            {
                //HeroController.instance.current_velocity = new Vector2(0, 15f);
                yield return null;
            }
            
            HeroController.instance.spellControl.Fsm.SetState("Quake Antic");

            autoDashing = false;
            StartCoroutine(setInvulnFalseAfterTime(2.0f));
        }

        private IEnumerator blackmothTurboDash()
        {
            HeroActions direction = GameManager.instance.inputHandler.inputActions;
            float timeRunning = 0;
            float lastCheck = 0;
            
            // meme time. not the full length of the dash
            // but this is how it used to be in blackmoth... seriously
            const float checkEvery = 0.2f;

            while (direction.dash.IsPressed && autoDashing && timeRunning < maxTurboTime)
            {
                yield return null;
                timeRunning += Time.deltaTime;

                if (timeRunning > (lastCheck + checkEvery))
                {
                    dashConditionChecker(true);
                    lastCheck = timeRunning;
                }
                else
                {
                    doDash();
                }
                
                
            }
            
            // run until the player taps the dash button a second time.
            while (!direction.dash.IsPressed && autoDashing && timeRunning < maxTurboTime)
            {
                yield return null;
                timeRunning += Time.deltaTime;
                
                if (timeRunning > (lastCheck + checkEvery))
                {
                    dashConditionChecker(true);
                    lastCheck = timeRunning;
                } else
                {
                    doDash();
                }
            }
            
            log("Disabling turbo button because you pressed dash a second time. or ran out of time");
            autoDashing = false;
            turboDashing = false;
            StartCoroutine(setInvulnFalseAfterTime(0.5f));
            
            // gives them 0 dash right after for fun.
            dashAnticNumber = 5;

        }
        
        private float getDashLength()
        {
            return hasSharpShadowCached ?
                HeroController.instance.DASH_SPEED_SHARP : HeroController.instance.DASH_SPEED;
        }


        private void dashConditionChecker(bool calledAutomatically)
        {
            if (autoDashing && !calledAutomatically)
            {
                return;
            }
            hasSharpShadowCached = PlayerData.instance.GetBool("equippedCharm_16");
            getPrivateField("dashQueueSteps").SetValue(HeroController.instance, 0);
            getPrivateField("dashQueuing").SetValue(HeroController.instance, false);
            HeroActions direction = GameManager.instance.inputHandler.inputActions;
            
            if (direction.up.IsPressed)
            {
                dashDirection.y = 1f;
            }
            else if (direction.down.IsPressed && !HeroController.instance.cState.onGround)
            {
                dashDirection.y = -1f;
            }
            else
            {
                dashDirection.y = 0;
            }

            if (direction.right.IsPressed)
            {
                dashDirection.x = 1f;
            }
            else if (direction.left.IsPressed)
            {
                dashDirection.x = -1f;
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            else if (dashDirection.y == 0)
            {
                dashDirection.x = HeroController.instance.cState.facingRight ? 1 : -1;
            }
            else
            {
                dashDirection.x = 0;
            }
            
            doDash();
            
            // Fixes TC problem where after tink sharp shadow is broken
            // yes even this shitty meme mod fixes TC bugs.
            sharpShadowFSM.SetState("Idle");

        }
        
        
        private bool dashTapped()
        {
            if (!completedCoroutines)
            {
                dashDirection = Vector2.zero;
                return false;
            }

            if (!autoDashing)
            {
                dashAnticNumber = rng.Next(0, TOTAL_DASH_ANTICS);
                dashAntics.resetAngelMemes();
            }
            
            dashConditionChecker(false);
            
            return true;
        }
        
        private void doDash()
        {
            if ( (!autoDashing || turboDashing) && (HeroController.instance.hero_state == ActorStates.no_input ||
                 HeroController.instance.hero_state == ActorStates.hard_landing ||
                 HeroController.instance.hero_state == ActorStates.dash_landing || !(dashCooldown <= 0f) ||
                 HeroController.instance.cState.dashing || HeroController.instance.cState.backDashing ||
                 (HeroController.instance.cState.attacking &&
                  !((float) getPrivateField("attack_time").GetValue(HeroController.instance) >=
                    HeroController.instance.ATTACK_RECOVERY_TIME)) || HeroController.instance.cState.preventDash ||
                 (!HeroController.instance.cState.onGround && airDashed() &&
                  !HeroController.instance.cState.wallSliding))) return;
            
            
            
            if ((!HeroController.instance.cState.onGround || Math.Abs(dashDirection.y) > 0.001f) && !HeroController.instance.inAcid)
            {
                getPrivateField("airDashed").SetValue(HeroController.instance, true);
            }

            invokePrivateMethod("ResetAttacksDash");
            invokePrivateMethod("CancelBounce");
            ((HeroAudioController)getPrivateField("audioCtrl").GetValue(HeroController.instance)).StopSound(HeroSounds.FOOTSTEPS_RUN);
            ((HeroAudioController)getPrivateField("audioCtrl").GetValue(HeroController.instance)).StopSound(HeroSounds.FOOTSTEPS_WALK);
            ((HeroAudioController)getPrivateField("audioCtrl").GetValue(HeroController.instance)).StopSound(HeroSounds.DASH);
            invokePrivateMethod("ResetLook");

            HeroController.instance.cState.recoiling = false;
                if (HeroController.instance.cState.wallSliding)
            {
                HeroController.instance.FlipSprite();
                if (HeroController.instance.cState.facingRight && Math.Abs(dashDirection.y) < 0.001f )
                {
                    dashDirection.x = 1f;
                } else if (Math.Abs(dashDirection.y) < 0.001f)
                {
                    dashDirection.x = -1f;
                }
            }
            else if (GameManager.instance.inputHandler.inputActions.right.IsPressed)
            {
                HeroController.instance.FaceRight();
            }
            else if (GameManager.instance.inputHandler.inputActions.left.IsPressed)
            {
                HeroController.instance.FaceLeft();
            }
            HeroController.instance.cState.dashing = true;
            getPrivateField("dashQueueSteps").SetValue(HeroController.instance, 0);
            HeroController.instance.dashBurst.transform.localPosition = new Vector3(4.11f, -0.55f, 0.001f);
            HeroController.instance.dashBurst.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            HeroController.instance.dashingDown = false;

                
            dashCooldown = dashCooldownTime;
                

            getPrivateField("shadowDashTimer").SetValue(HeroController.instance, getPrivateField("dashCooldownTimer").GetValue(HeroController.instance));
            HeroController.instance.proxyFSM.SendEvent("HeroCtrl-ShadowDash");
            HeroController.instance.cState.shadowDashing = true;
            ((AudioSource)getPrivateField("audioSource").GetValue(HeroController.instance)).PlayOneShot(HeroController.instance.sharpShadowClip, 1f);
            HeroController.instance.sharpShadowPrefab.SetActive(true);

            if (HeroController.instance.cState.shadowDashing)
            {
                if (HeroController.instance.transform.localScale.x > 0f)
                {
                    getPrivateField("dashEffect").SetValue(HeroController.instance, HeroController.instance.shadowdashBurstPrefab.Spawn(new Vector3(HeroController.instance.transform.position.x + 5.21f, HeroController.instance.transform.position.y - 0.58f, HeroController.instance.transform.position.z + 0.00101f)));
                    ((GameObject)getPrivateField("dashEffect").GetValue(HeroController.instance)).transform.localScale = new Vector3(1.919591f, ((GameObject)getPrivateField("dashEffect").GetValue(HeroController.instance)).transform.localScale.y, ((GameObject)getPrivateField("dashEffect").GetValue(HeroController.instance)).transform.localScale.z);
                }
                else
                {
                    getPrivateField("dashEffect").SetValue(HeroController.instance, HeroController.instance.shadowdashBurstPrefab.Spawn(new Vector3(HeroController.instance.transform.position.x - 5.21f, HeroController.instance.transform.position.y - 0.58f, HeroController.instance.transform.position.z + 0.00101f)));
                    ((GameObject)getPrivateField("dashEffect").GetValue(HeroController.instance)).transform.localScale = new Vector3(-1.919591f, ((GameObject)getPrivateField("dashEffect").GetValue(HeroController.instance)).transform.localScale.y, ((GameObject)getPrivateField("dashEffect").GetValue(HeroController.instance)).transform.localScale.z);
                }
                HeroController.instance.shadowRechargePrefab.SetActive(true);
                FSMUtility.LocateFSM(HeroController.instance.shadowRechargePrefab, "Recharge Effect").SendEvent("RESET");
                ParticleSystem ps = HeroController.instance.shadowdashParticlesPrefab.GetComponent<ParticleSystem>();
                ParticleSystem.EmissionModule em = ps.emission;
                em.enabled = true;
                HeroController.instance.shadowRingPrefab.Spawn(HeroController.instance.transform.position);
            }
            else
            {
                HeroController.instance.dashBurst.SendEvent("PLAY");
                ParticleSystem ps = HeroController.instance.dashParticlesPrefab.GetComponent<ParticleSystem>();
                ParticleSystem.EmissionModule em = ps.emission;
                em.enabled = true;
            }
            // ReSharper disable once InvertIf because it looks dumb
            if (HeroController.instance.cState.onGround && !HeroController.instance.cState.shadowDashing)
            {
                getPrivateField("dashEffect").SetValue(HeroController.instance, HeroController.instance.backDashPrefab.Spawn(HeroController.instance.transform.position));
                ((GameObject)getPrivateField("dashEffect").GetValue(HeroController.instance)).transform.localScale = new Vector3(HeroController.instance.transform.localScale.x * -1f, HeroController.instance.transform.localScale.y, HeroController.instance.transform.localScale.z);
            }
        }

        private bool airDashed()
        {
            return (bool) getPrivateField("airDashed").GetValue(HeroController.instance);
        }

        
        private static void log(string str)
        {
            Modding.Logger.Log("[Shitmothst] " + str);
        }
        
        
        private FieldInfo getPrivateField(string fieldName)
        {
            if (!privateFields.ContainsKey(fieldName))
            {
                privateFields.Add(fieldName,
                    HeroController.instance.GetType()
                        .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance));
            }
            return privateFields[fieldName];
        }

        private void invokePrivateMethod(string methodName)
        {
            if (!privateMethods.ContainsKey(methodName))
            {
                privateMethods.Add(methodName,
                    HeroController.instance.GetType()
                        .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance));
            }
            privateMethods[methodName]?.Invoke(HeroController.instance, new object[] { });
        }
        
        private Dictionary<string, FieldInfo> privateFields;
        private Dictionary<string, MethodInfo> privateMethods;

    }
}