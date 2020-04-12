using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseTextureExample : MonoBehaviour
{
    //シード
    private float _seedX, _seedZ;

    //マップのサイズ
    [SerializeField]
    [Header("------実行中に変えれない------")]
    private float _width = 50;
    [SerializeField]
    private float _depth = 50;

    //コライダーが必要か
    [SerializeField]
    private bool _needToCollider = false;

    //高さの最大値
    [SerializeField]
    [Header("------実行中に変えられる------")]
    private float _maxHeight = 10;

    //パーリンノイズを使ったマップか
    [SerializeField]
    private bool _isPerlinNoiseMap = true;

    //起伏の激しさ
    [SerializeField]
    private float _relief = 15f;

    //Y座標を滑らかにするか(小数点以下をそのままにする)
    [SerializeField]
    private bool _isSmoothness = false;

    //マップの大きさ
    [SerializeField]
    private float _mapSize = 1f;

    private MeshRenderer[,] _cubes = null;

    //=================================================================================
    //初期化
    //=================================================================================

    private void Awake()
    {
        _cubes = new MeshRenderer[(int)_width,(int)_depth];
        //マップサイズ設定
        transform.localScale = new Vector3(_mapSize, _mapSize, _mapSize);

        //同じマップにならないようにシード生成
        _seedX = Random.value * 100f;
        _seedZ = Random.value * 100f;

        //キューブ生成
        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _depth; z++)
            {
                //新しいキューブ作成、平面に置く
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localPosition = new Vector3(x, 0, z);
                cube.transform.SetParent(transform);
                _cubes[x, z] = cube.GetComponent<MeshRenderer>();

                if (!_needToCollider)
                {
                    Destroy(cube.GetComponent<BoxCollider>());
                }

                //高さ設定
                SetY(cube);
            }
        }
    }

    //インスペクターの値が変更された時
    private void OnValidate()
    {
        //実行中でなければスルー
        if (!Application.isPlaying)
        {
            return;
        }

        //マップの大きさ設定
        transform.localScale = new Vector3(_mapSize, _mapSize, _mapSize);

        //各キューブのY座標変更
        foreach (Transform child in transform)
        {
            SetY(child.gameObject);
        }
    }

    //キューブのY座標を設定する
    private void SetY(GameObject cube)
    {
        float y = 0;

        //パーリンノイズを使って高さを決める場合
        if (_isPerlinNoiseMap)
        {
            float xSample = (cube.transform.localPosition.x + _seedX) / _relief;
            float zSample = (cube.transform.localPosition.z + _seedZ) / _relief;

            float noise = Mathf.PerlinNoise(xSample, zSample);

            y = _maxHeight * noise;
        }
        //完全ランダムで高さを決める場合
        else
        {
            y = Random.Range(0, _maxHeight);
        }

        //滑らかに変化しない場合はyを四捨五入
        if (!_isSmoothness)
        {
            y = Mathf.Round(y);
        }

        //位置設定
        cube.transform.localPosition = new Vector3(cube.transform.localPosition.x, y, cube.transform.localPosition.z);

        //高さによって色を段階的に変更
        Color color = Color.black;//岩盤っぽい色

        if (y > _maxHeight * 0.3f)
        {
            ColorUtility.TryParseHtmlString("#019540FF", out color);//草っぽい色
        }
        else if (y > _maxHeight * 0.2f)
        {
            ColorUtility.TryParseHtmlString("#2432ADFF", out color);//水っぽい色
        }
        else if (y > _maxHeight * 0.1f)
        {
            ColorUtility.TryParseHtmlString("#D4500EFF", out color);//マグマっぽい色
        }

        cube.GetComponent<MeshRenderer>().material.color = color;
    }

    private Texture2D CreateTexture()
    {
        Texture2D tex = new Texture2D((int)_width, (int)_depth);
        for (int x = 0; x < _cubes.GetLength(0); ++x)
        {
            for (int z = 0; z < _cubes.GetLength(1); ++z)
            {
                tex.SetPixel(x, z, _cubes[x, z].material.color);
            }
        }
        return tex;
    }

    private void CreatePng(Texture2D tex)
    {
        byte[] pngData = tex.EncodeToPNG();   // pngのバイト情報を取得.

        // ファイルダイアログの表示.
        string filePath = UnityEditor.EditorUtility.SaveFilePanel("Save Texture", "", tex.name + ".png", "png");

        if (filePath.Length > 0)
        {
            // pngファイル保存.
            System.IO.File.WriteAllBytes(filePath, pngData);
        }
    }

    public void OnClickButton()
    {
        CreatePng(CreateTexture());
    }
}
