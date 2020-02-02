using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusGUI : MonoBehaviour
{
    public Sprite _spriteGood;
    public Sprite _spriteBad;
    public Sprite _spriteUnknown;
    public Sprite _spriteRepair;

    float _AnimTimer = 0;

    public Sprite _changeToSprite;
    public UnityEngine.UI.Image _targetImage;

    bool _isActive = false;

    Vector2 _ImageTargetPos;
    Vector2 _ImageTargetSize;
    // Start is called before the first frame update
    void Start()
    {
        _targetImage = GetComponent<UnityEngine.UI.Image>();
        _ImageTargetPos = _targetImage.transform.position;
        //_ImageTargetSize = _targetImage.
    }

    public void ShowStatusFor(ComponentBase comp)
    {
        if(!comp)
        {
            _isActive = false;
        }
        if(comp._hasHiddenWorkingStatus)
        {
            _isActive = true;
            _changeToSprite = _spriteUnknown;
        }
        else if(comp._isWorking)
        {
            _isActive = true;
            _changeToSprite = _spriteGood;
        }
        else
        {
            _isActive = true;
            _changeToSprite = _spriteBad;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(_isActive)
        {
            if(_changeToSprite)
            {
                _AnimTimer -= Time.deltaTime*2.0f;
                if(_AnimTimer <= 0)
                {
                    _targetImage.sprite = _changeToSprite;
                    _changeToSprite = null;
                }
            }
            else
            {
                if(_AnimTimer < 1.0f)
                {
                    _AnimTimer += Time.deltaTime * (3.0f - _AnimTimer * 1.8f); //faster in the beginning
                    if (_AnimTimer > 1)
                    {
                        _AnimTimer = 1;
                    }
                }
            }

            float h = Tweens.Back.InOut(_AnimTimer);
            float w = Tweens.Elastic.InOut(_AnimTimer);
            _targetImage.transform.localScale = new Vector3(w, h, 1);
        }
        else
        {
            if (_AnimTimer > 0)
            {
                _AnimTimer -= Time.deltaTime*3.0f;
                if (_AnimTimer <= 0)
                {
                    _AnimTimer = 0;
                }
            }
        }
        if (_AnimTimer == 1)
        {
            _targetImage.enabled = true;
            _targetImage.transform.localScale = new Vector3(1, 1, 1);
            _targetImage.color = new Color(1, 1, 1, 1);
        }
        else if (_AnimTimer > 0)
        {
            _targetImage.enabled = true;
            float h = Tweens.Back.InOut(_AnimTimer);
            float w = Tweens.Elastic.InOut(_AnimTimer);
            _targetImage.transform.localScale = new Vector3(w, h, 1);
            _targetImage.color = new Color(1, 1, 1, _AnimTimer* _AnimTimer);
        }
        else
        {
            _targetImage.enabled = false;
        }
    }
}
