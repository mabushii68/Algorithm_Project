using UnityEngine;
using System.Collections.Generic;

public class ObjectPlacer : MonoBehaviour
{
    // --- ���� ���� ---
    [Header("Placement Settings")]
    [Tooltip("��ġ�� ������Ʈ�� ������ ���")]
    public List<GameObject> placeablePrefabs; // ��ġ�� �� �ִ� ������ ���
    [Tooltip("������Ʈ�� ��ġ�� �� �������� ���� ���̾� (�ٴ� ���̾�)")]
    public LayerMask groundLayer; // "Ground" ���̾ �����ϵ��� �ν����Ϳ��� ����

    // --- ���� ���� ���� ---
    private GameObject currentPickedObject = null; // ���� ��� �ִ� (���õ�) ������Ʈ
    private float initialObjectYOffsetFromCamera; // ������Ʈ ���� �� ī�޶�κ����� �ʱ� ���� (�Ÿ�)

    private Camera mainCamera; // ���� ī�޶� ����

    void Awake()
    {
        mainCamera = Camera.main; // ���� ���� ī�޶� ã�ƿɴϴ�.
        if (mainCamera == null)
        {
            Debug.LogError("���� 'MainCamera' �±װ� ������ ī�޶� �����ϴ�! �ùٸ� ī�޶� �±׸� �������ּ���.");
        }
    }

    void Update()
    {
        // 1. ���콺 ���� ��ư Ŭ�� ���� (������Ʈ ���� �Ǵ� ���� �õ�)
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseDown();
        }

        // 2. ���콺 ���� ��ư ������ �ִ� ���� (������Ʈ �̵�)
        if (Input.GetMouseButton(0) && currentPickedObject != null)
        {
            HandleMouseDrag();
        }

        // 3. ���콺 ���� ��ư ���� ���� (������Ʈ ��ġ �Ϸ�)
        if (Input.GetMouseButtonUp(0) && currentPickedObject != null)
        {
            HandleMouseUp();
        }

        // --- (�߰� ���) ��Ŭ�� �� ���� ��� �ִ� ������Ʈ �ı� ---
        if (Input.GetMouseButtonDown(1) && currentPickedObject != null)
        {
            Destroy(currentPickedObject);
            currentPickedObject = null;
            Debug.Log("������Ʈ �ı�: ��Ŭ��");
        }
    }

    // ���콺 ���� ��ư�� ������ �� ó��
    void HandleMouseDown()
    {
        // ���� ��� �ִ� ������Ʈ�� ���� ���� ���ο� ������Ʈ ����/���� �õ�
        if (currentPickedObject == null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // ����ĳ��Ʈ (��� ���̾ ����)
            if (Physics.Raycast(ray, out hit))
            {
                // ** ù ��° ����: �浹�� ������Ʈ�� "Ground" ���̾��� ���ο� ������Ʈ ���� **
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    if (placeablePrefabs != null && placeablePrefabs.Count > 0)
                    {
                        // ������ �������� �ٿ���� Y �������� ����Ͽ� �� ���� ��Ȯ�� ���̰� ��
                        Collider prefabCollider = placeablePrefabs[0].GetComponent<Collider>();
                        float spawnYOffset = 0f;
                        if (prefabCollider != null)
                        {
                            // �ݶ��̴��� �߽ɿ��� �ٴڱ����� �Ÿ� (���� ����)
                            spawnYOffset = prefabCollider.bounds.extents.y;
                        }

                        // ������Ʈ�� �� ǥ�� Y ���� ���� ���̸�ŭ �ö�ͼ� �����ǵ��� ��ġ ����
                        Vector3 spawnPosition = new Vector3(hit.point.x, hit.point.y + spawnYOffset, hit.point.z);

                        currentPickedObject = Instantiate(placeablePrefabs[0], spawnPosition, Quaternion.identity);
                        currentPickedObject.name = placeablePrefabs[0].name + "_Instance";
                        currentPickedObject.tag = "PlaceableAsset"; // ���� ������ ������Ʈ�� �±� ����!

                        Debug.Log("�� ������Ʈ ����: " + currentPickedObject.name);

                        // ��� �ִ� ���� Y�� �̵��� ���� ī�޶�κ����� �ʱ� ���� ���
                        initialObjectYOffsetFromCamera = mainCamera.WorldToScreenPoint(currentPickedObject.transform.position).z;
                    }
                    else
                    {
                        Debug.LogWarning("��ġ�� �������� placeablePrefabs ����Ʈ�� �����ϴ�!");
                    }
                }
                // ** �� ��° ����: �浹�� ������Ʈ�� "Ground" ���̾ �ƴϰ�, "PlaceableAsset" �±׸� ������ �ִٸ� ���� ������Ʈ ���� **
                // �� 'else if' ������ ���� �߿��մϴ�. Ground�� �ƴ� �ٸ� ��� ������Ʈ�� ������ ���� �ʰ� �մϴ�.
                else if (hit.collider.gameObject.CompareTag("PlaceableAsset"))
                {
                    currentPickedObject = hit.collider.gameObject;
                    Debug.Log("���� ������Ʈ ����: " + currentPickedObject.name);

                    // ��� �ִ� ���� Y�� �̵��� ���� ī�޶�κ����� �ʱ� ���� ���
                    initialObjectYOffsetFromCamera = mainCamera.WorldToScreenPoint(currentPickedObject.transform.position).z;
                }
                // ** �� ���� ��� ��� (Ground�� PlaceableAsset�� �ƴ� ������Ʈ Ŭ��) **
                else
                {
                    Debug.Log("������ �� ���� ������Ʈ Ŭ��: " + hit.collider.gameObject.name);
                    currentPickedObject = null; // � ������Ʈ�� ���� ����
                }
            }
            else // ���̰� �ƹ��͵� ������ ������ �� (�� �ϴ� Ŭ��)
            {
                currentPickedObject = null; // � ������Ʈ�� ���� ����
            }
        }
    }

    // ���콺 ���� ��ư�� ������ �ִ� ���� ó�� (������Ʈ �̵�)
    void HandleMouseDrag()
    {
        // ���� ���콺 ��ġ (��ũ�� ��ǥ)�� �����ɴϴ�.
        // X, Y�� ���콺 Ŀ���� ȭ�� ��ġ, Z�� ������ ����� ������Ʈ�� ī�޶�κ����� �����Դϴ�.
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, initialObjectYOffsetFromCamera);

        // ��ũ�� ��ǥ�� 3D ���� ��ǥ�� ��ȯ�Ͽ� ������Ʈ�� ���ο� ��ġ�� �����մϴ�.
        // �� ��ȯ�� ���콺 Ŀ���� 3D ���� ��ȭ�� ���� ������Ʈ�� �Բ� ���Ʒ��� �����̰� �մϴ�.
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        // ������Ʈ�� ��ġ�� ������Ʈ�մϴ�.
        currentPickedObject.transform.position = worldPosition;
    }

    // ���콺 ���� ��ư�� ������ �� ó�� (������Ʈ ��ġ �Ϸ�)
    void HandleMouseUp()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // ������Ʈ�� ���� ���� ���콺 �Ʒ��� "Ground" ���̾ �ִ��� �˻��Ͽ� Y���� ��Ȯ�� �����մϴ�.
        // Physics.Raycast�� ������ ���ڿ� groundLayer ����ũ�� ����Ͽ� ���� Ground ���̾ �˻�
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            Collider objCollider = currentPickedObject.GetComponent<Collider>();
            if (objCollider != null)
            {
                // ������Ʈ �ݶ��̴��� �ٿ�� �ڽ����� ���� ���� Y ��ǥ�� ���մϴ�.
                float objectBottomY = objCollider.bounds.min.y;

                // ������Ʈ�� ���� �Ǻ� Y ��ġ�� ������Ʈ �ٴ� Y�� ���� (������)�� ����մϴ�.
                float offsetFromPivotToBottom = currentPickedObject.transform.position.y - objectBottomY;

                // ���̰� �浹�� �ٴ� ������ Y ��ǥ�� �� �������� ���Ͽ� ������Ʈ �Ǻ��� ��ǥ Y�� �����մϴ�.
                // �̷��� �ϸ� ������Ʈ�� �ٴ��� �� ǥ�鿡 ��Ȯ�� ��� �˴ϴ�.
                float targetY = hit.point.y + offsetFromPivotToBottom;

                // ������Ʈ�� ���� ��ġ ��ġ�� �����մϴ�. (X, Z�� ���� ��ġ ����, Y�� ����)
                currentPickedObject.transform.position = new Vector3(currentPickedObject.transform.position.x, targetY, currentPickedObject.transform.position.z);

                Debug.Log("������Ʈ �ٴڿ� ����: " + currentPickedObject.name + ", ���� Y: " + targetY);
            }
            else
            {
                Debug.LogWarning("��ġ�� ������Ʈ�� Collider�� ���� Y�� ������ �� �� �����ϴ�: " + currentPickedObject.name);
            }
        }
        // else: ���콺�� ���� �ƴ� �� ������ ���� �� ������, ������Ʈ�� ���� ���߿� �� �ִ� Y ��ġ�� �״�� ��ġ�˴ϴ�.
        //       (��, Y�� ���� ������ ������� �ʽ��ϴ�.)

        Debug.Log("������Ʈ ��ġ �Ϸ�: " + currentPickedObject.name);
        currentPickedObject = null; // ������Ʈ ���� ����
    }
}