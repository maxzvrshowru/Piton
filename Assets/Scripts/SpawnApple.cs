using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnApple : MonoBehaviour
{
    public float spawnTime = 3.0f; // Пауза до следующего яблока
    private float previousTime; // Время съедания предыдущего яблока
    private bool waitSpawn = false; // Включаем таймер на время ожидания

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Яблоко съедено (объект не рисуется на столе), но таймер не запущен
        if (!waitSpawn && !gameObject.GetComponent<SpriteRenderer>().enabled)
        {
            previousTime = Time.time; // Выставляем момент времени, что яблоко съедено
            waitSpawn = true; // Запускаем таймер
        }

        // Таймер запущен, ждем когда он подойдет ко времени
        // Чтобы показать яблоко
        if (waitSpawn && Time.time - previousTime > spawnTime)
        {
            waitSpawn = false; // выполняем отключение таймера
            // Получаем новые случайные координаты яблока на поле
            transform.position = new Vector3(Random.Range(0,MovePiton.width), Random.Range(0, MovePiton.height));
            // Показываем яблоко
            gameObject.GetComponent<SpriteRenderer>().enabled = true;
        }
    }
}
