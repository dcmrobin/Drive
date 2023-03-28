using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    [SerializeField]
    RectTransform _texture;
    [SerializeField]
    GameObject _sphereTest;
    [SerializeField]
    Texture2D _refSprite;

    public void OnClickPickerColor()
    {
        SetColor();
    }

    private void SetColor()
    {
        Vector3 imagePos = _texture.position;
        float globalPosX = Input.mousePosition.x - imagePos.x;
        float globalPosY = Input.mousePosition.y - imagePos.y;

        int localPosX = (int) (globalPosX*(_refSprite.width/_texture.rect.width*2.35));
        int localPosY = (int) (globalPosY*(_refSprite.height/_texture.rect.height*2.35));

        Color c = _refSprite.GetPixel(localPosX, localPosY);
        SetActualColor(c);
    }

    void SetActualColor(Color c)
    {
        _sphereTest.GetComponent<Image>().color = c;
    }
}
