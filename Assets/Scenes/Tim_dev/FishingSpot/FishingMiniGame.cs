using UnityEngine;
using UnityEngine.UI;

public class FishingMiniGame : MonoBehaviour
{
    [SerializeField] private Transform topPivot;
    [SerializeField] private Transform bottomPivot;
    [SerializeField] private Transform fish;

    float fishPosition;
    float fishDestinatiion;

    float fishTimer;
    [SerializeField] float timerMultiplicator = 3f;
    
    float fishSpeed;
    [SerializeField] float smoothMotion = 1f;

    [SerializeField] Transform hook;

    float hookPosition;
    [SerializeField] float hookSize = 0.1f;
    [SerializeField] float hookPower = 0.1f;
    
    float hookProgress;
    float hookPullVelocity;
    [SerializeField] float hookPullPower = 0.01f;
    [SerializeField] float hookGravityPower = 0.02f;
    [SerializeField] float maxHookVelocity = 0.5f;
    [SerializeField] float hookProgressDegradationPower = 0.1f;

    [SerializeField] SpriteRenderer hookSpriteRenderer;

    [SerializeField] Transform progressBarContainer;

    bool pause = false;

    [SerializeField] float failTimer = 10f;

    private void Start()
    {
        Resize();
    }



    private void Update()
    {
        if(pause) return;
        Fish();
        Hook();
        ProgressCheck();
    }

    private void Resize()
    {
        Bounds b = hookSpriteRenderer.bounds;;
        float ySize = b.size.y;
        Vector3 ls = hook.localScale;
        float distance = Vector3.Distance(topPivot.position, bottomPivot.position);
        ls.y = (distance / ySize * hookSize);
        hook.localScale = ls;
    }

    void ProgressCheck()
    {
        // 显示钓鱼进度（水平方向）
        Vector3 ls = progressBarContainer.localScale;
        ls.x = hookProgress;
        progressBarContainer.localScale = ls;

        float min = hookPosition - hookSize / 2;
        float max = hookPosition + hookSize / 2;

        if(fishPosition >= min && fishPosition < max)
        {
            hookProgress += hookPower * Time.deltaTime;
        }
        else{
            hookProgress -= hookProgressDegradationPower * Time.deltaTime;

            failTimer -= Time.deltaTime;
            if(failTimer <= 0f){
                Lose();
            }
        }
        if(hookProgress >= 1f)
        {
            Win();
        }

        hookProgress = Mathf.Clamp(hookProgress, 0f, 1f);
    }

    private void Win()
    {
        pause = true;
        Debug.Log("You Win! You get a fish!");
    }

    private void Lose()
    {
        pause = true;
        Debug.Log("You Lose! You get nothing!");
    }

    void Hook()
    {
        if(Input.GetMouseButton(0))
        {
            hookPullVelocity += hookPower * Time.deltaTime;
        }
        hookPullVelocity -= hookGravityPower * Time.deltaTime;
        
        // 限制钩子速度
        hookPullVelocity = Mathf.Clamp(hookPullVelocity, -maxHookVelocity, maxHookVelocity);

        hookPosition += hookPullVelocity;
        
        // 限制钩子在可视区域内，考虑钩子大小
        float maxHookPosition = 1f - hookSize;
        
        // 如果钩子触顶，立即重置速度
        if (hookPosition >= maxHookPosition)
        {
            hookPosition = maxHookPosition;
            hookPullVelocity = 0f;
        }
        // 如果钩子触底，也重置速度
        else if (hookPosition <= 0f)
        {
            hookPosition = 0f;
            hookPullVelocity = 0f;
        }
        
        hook.position = Vector3.Lerp(bottomPivot.position, topPivot.position, hookPosition);
    }

    void Fish(){
        fishTimer -= Time.deltaTime;
        if (fishTimer <= 0f)
        {
            fishTimer = UnityEngine.Random.value * timerMultiplicator;

            fishDestinatiion = UnityEngine.Random.value;
        }

        fishPosition = Mathf.SmoothDamp(fishPosition, fishDestinatiion, ref fishSpeed, smoothMotion);

        // fishPosition = Mathf.MoveTowards(fishPosition, fishDestinatiion, Time.deltaTime * fishSpeed);
        fish.position = Vector3.Lerp(bottomPivot.position, topPivot.position, fishPosition);
    }


}