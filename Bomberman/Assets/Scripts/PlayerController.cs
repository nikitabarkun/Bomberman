using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private GameObject bombPrefab;

    private float _bombExplosionRadius = 3f;
    private float _bombTime = 3f;
    
    private bool _isMoving;
    private bool _isBombPlaced;
    
    private void Update()
    {
        if (_isMoving)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveTo(Vector3.up);
        }
        
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveTo(Vector3.down);
        }
        
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveTo(Vector3.left);
        }
        
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveTo(Vector3.right);
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!_isBombPlaced)
            {
                _isBombPlaced = true;
                StartCoroutine(PlaceBomb());
            }
        }
    }
    
    private void MoveTo(Vector3 direction)
    {
        if (Raycast(direction))
        {
            return;
        }

        _isMoving = true;

        var position = transform.position + direction;

        transform.rotation = Quaternion.LookRotation(direction, Vector3.back);
        
        transform.DOMove(position, 0.5f).OnComplete(() =>
        {
            _isMoving = false;
        });
    }
    
    private bool Raycast(Vector3 direction)
    {
        Ray ray = new Ray(transform.position, direction);
        Physics.Raycast(ray, out var hit, 1f);
        
        return hit.collider != null;
    }

    private IEnumerator PlaceBomb()
    {
        GameObject bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        bomb.GetComponent<Renderer>().material.DOColor(Color.red, _bombTime);
        
        yield return new WaitForSeconds(_bombTime);

        TryDestroy(bomb, Vector3.up);
        TryDestroy(bomb, Vector3.down);
        TryDestroy(bomb, Vector3.left);
        TryDestroy(bomb, Vector3.right);
        
        Explode(bomb);
    }

    private void TryDestroy(GameObject bomb, Vector3 direction)
    {
        Ray ray = new Ray(bomb.transform.position, direction);
        Physics.Raycast(ray, out var hit, _bombExplosionRadius);

        if (hit.collider != null && hit.collider.gameObject.CompareTag("Destructible"))
        {
            Destroy(hit.collider.gameObject);
        }
    }

    private void Explode(GameObject bomb)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Join(bomb.transform.DOScale(Vector3.one * 3f, 0.1f));
        sequence.Join(bomb.GetComponent<Renderer>().material.DOColor(Color.clear, 0.1f));

        sequence.OnComplete(() =>
        {
            _isBombPlaced = false;
            Destroy(bomb);
        });
    }
}
