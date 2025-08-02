using UnityEngine;
using System.Collections.Generic;

public class ObjectPlacer : MonoBehaviour
{
    // --- 설정 변수 ---
    [Header("Placement Settings")]
    [Tooltip("배치할 오브젝트의 프리팹 목록")]
    public List<GameObject> placeablePrefabs; // 배치할 수 있는 프리팹 목록
    [Tooltip("오브젝트가 배치될 때 기준으로 삼을 레이어 (바닥 레이어)")]
    public LayerMask groundLayer; // "Ground" 레이어를 선택하도록 인스펙터에서 설정

    // --- 내부 상태 변수 ---
    private GameObject currentPickedObject = null; // 현재 들고 있는 (선택된) 오브젝트
    private float initialObjectYOffsetFromCamera; // 오브젝트 선택 시 카메라로부터의 초기 깊이 (거리)

    private Camera mainCamera; // 메인 카메라 참조

    void Awake()
    {
        mainCamera = Camera.main; // 씬의 메인 카메라를 찾아옵니다.
        if (mainCamera == null)
        {
            Debug.LogError("씬에 'MainCamera' 태그가 지정된 카메라가 없습니다! 올바른 카메라에 태그를 지정해주세요.");
        }
    }

    void Update()
    {
        // 1. 마우스 왼쪽 버튼 클릭 감지 (오브젝트 선택 또는 생성 시도)
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseDown();
        }

        // 2. 마우스 왼쪽 버튼 누르고 있는 동안 (오브젝트 이동)
        if (Input.GetMouseButton(0) && currentPickedObject != null)
        {
            HandleMouseDrag();
        }

        // 3. 마우스 왼쪽 버튼 떼기 감지 (오브젝트 배치 완료)
        if (Input.GetMouseButtonUp(0) && currentPickedObject != null)
        {
            HandleMouseUp();
        }

        // --- (추가 기능) 우클릭 시 현재 들고 있는 오브젝트 파괴 ---
        if (Input.GetMouseButtonDown(1) && currentPickedObject != null)
        {
            Destroy(currentPickedObject);
            currentPickedObject = null;
            Debug.Log("오브젝트 파괴: 우클릭");
        }
    }

    // 마우스 왼쪽 버튼을 눌렀을 때 처리
    void HandleMouseDown()
    {
        // 현재 들고 있는 오브젝트가 없을 때만 새로운 오브젝트 선택/생성 시도
        if (currentPickedObject == null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 레이캐스트 (모든 레이어에 대해)
            if (Physics.Raycast(ray, out hit))
            {
                // ** 첫 번째 조건: 충돌한 오브젝트가 "Ground" 레이어라면 새로운 오브젝트 생성 **
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    if (placeablePrefabs != null && placeablePrefabs.Count > 0)
                    {
                        // 생성될 프리팹의 바운즈에서 Y 오프셋을 계산하여 땅 위에 정확히 놓이게 함
                        Collider prefabCollider = placeablePrefabs[0].GetComponent<Collider>();
                        float spawnYOffset = 0f;
                        if (prefabCollider != null)
                        {
                            // 콜라이더의 중심에서 바닥까지의 거리 (절반 높이)
                            spawnYOffset = prefabCollider.bounds.extents.y;
                        }

                        // 오브젝트가 땅 표면 Y 위에 절반 높이만큼 올라와서 생성되도록 위치 조정
                        Vector3 spawnPosition = new Vector3(hit.point.x, hit.point.y + spawnYOffset, hit.point.z);

                        currentPickedObject = Instantiate(placeablePrefabs[0], spawnPosition, Quaternion.identity);
                        currentPickedObject.name = placeablePrefabs[0].name + "_Instance";
                        currentPickedObject.tag = "PlaceableAsset"; // 새로 생성된 오브젝트에 태그 적용!

                        Debug.Log("새 오브젝트 생성: " + currentPickedObject.name);

                        // 들고 있는 동안 Y축 이동을 위해 카메라로부터의 초기 깊이 계산
                        initialObjectYOffsetFromCamera = mainCamera.WorldToScreenPoint(currentPickedObject.transform.position).z;
                    }
                    else
                    {
                        Debug.LogWarning("배치할 프리팹이 placeablePrefabs 리스트에 없습니다!");
                    }
                }
                // ** 두 번째 조건: 충돌한 오브젝트가 "Ground" 레이어가 아니고, "PlaceableAsset" 태그를 가지고 있다면 기존 오브젝트 선택 **
                // 이 'else if' 조건이 가장 중요합니다. Ground가 아닌 다른 모든 오브젝트를 무작정 들지 않게 합니다.
                else if (hit.collider.gameObject.CompareTag("PlaceableAsset"))
                {
                    currentPickedObject = hit.collider.gameObject;
                    Debug.Log("기존 오브젝트 선택: " + currentPickedObject.name);

                    // 들고 있는 동안 Y축 이동을 위해 카메라로부터의 초기 깊이 계산
                    initialObjectYOffsetFromCamera = mainCamera.WorldToScreenPoint(currentPickedObject.transform.position).z;
                }
                // ** 그 외의 모든 경우 (Ground도 PlaceableAsset도 아닌 오브젝트 클릭) **
                else
                {
                    Debug.Log("선택할 수 없는 오브젝트 클릭: " + hit.collider.gameObject.name);
                    currentPickedObject = null; // 어떤 오브젝트도 들지 않음
                }
            }
            else // 레이가 아무것도 맞추지 못했을 때 (빈 하늘 클릭)
            {
                currentPickedObject = null; // 어떤 오브젝트도 들지 않음
            }
        }
    }

    // 마우스 왼쪽 버튼을 누르고 있는 동안 처리 (오브젝트 이동)
    void HandleMouseDrag()
    {
        // 현재 마우스 위치 (스크린 좌표)를 가져옵니다.
        // X, Y는 마우스 커서의 화면 위치, Z는 이전에 저장된 오브젝트의 카메라로부터의 깊이입니다.
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, initialObjectYOffsetFromCamera);

        // 스크린 좌표를 3D 월드 좌표로 변환하여 오브젝트의 새로운 위치로 설정합니다.
        // 이 변환은 마우스 커서의 3D 깊이 변화에 따라 오브젝트가 함께 위아래로 움직이게 합니다.
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        // 오브젝트의 위치를 업데이트합니다.
        currentPickedObject.transform.position = worldPosition;
    }

    // 마우스 왼쪽 버튼을 떼었을 때 처리 (오브젝트 배치 완료)
    void HandleMouseUp()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 오브젝트를 놓는 순간 마우스 아래에 "Ground" 레이어가 있는지 검사하여 Y축을 정확히 정렬합니다.
        // Physics.Raycast의 마지막 인자에 groundLayer 마스크를 사용하여 오직 Ground 레이어만 검사
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            Collider objCollider = currentPickedObject.GetComponent<Collider>();
            if (objCollider != null)
            {
                // 오브젝트 콜라이더의 바운딩 박스에서 가장 낮은 Y 좌표를 구합니다.
                float objectBottomY = objCollider.bounds.min.y;

                // 오브젝트의 현재 피봇 Y 위치와 오브젝트 바닥 Y의 차이 (오프셋)를 계산합니다.
                float offsetFromPivotToBottom = currentPickedObject.transform.position.y - objectBottomY;

                // 레이가 충돌한 바닥 지점의 Y 좌표에 이 오프셋을 더하여 오브젝트 피봇의 목표 Y를 설정합니다.
                // 이렇게 하면 오브젝트의 바닥이 땅 표면에 정확히 닿게 됩니다.
                float targetY = hit.point.y + offsetFromPivotToBottom;

                // 오브젝트의 최종 배치 위치를 설정합니다. (X, Z는 현재 위치 유지, Y만 조정)
                currentPickedObject.transform.position = new Vector3(currentPickedObject.transform.position.x, targetY, currentPickedObject.transform.position.z);

                Debug.Log("오브젝트 바닥에 정렬: " + currentPickedObject.name + ", 최종 Y: " + targetY);
            }
            else
            {
                Debug.LogWarning("배치된 오브젝트에 Collider가 없어 Y축 정렬을 할 수 없습니다: " + currentPickedObject.name);
            }
        }
        // else: 마우스가 땅이 아닌 빈 공간에 있을 때 놓으면, 오브젝트는 현재 공중에 떠 있는 Y 위치에 그대로 배치됩니다.
        //       (즉, Y축 정렬 로직이 실행되지 않습니다.)

        Debug.Log("오브젝트 배치 완료: " + currentPickedObject.name);
        currentPickedObject = null; // 오브젝트 선택 해제
    }
}