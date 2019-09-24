using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovePiton : MonoBehaviour
{
    // Перечисляем направления
    enum Direction : byte { Up, Left, Down, Right };

    // Задаем начальное направление движения
    private Direction pitonMoveDirection = Direction.Down;

    // Задаем границы поля игры
    public const int height = 21;
    public const int width = 21;
    
    // Начальная скорость питона
    public float fallSetupTime = 0.3f;
    private float fallTime;

    // Предыдущее время, на котором змея подвинулась
    private float previousTime;

    // Бок питона и начальное количество блоков
    public byte pitonPartStartCount;
    public GameObject pitonPart;

    // Если игра закончилась
    private bool endGame = false;

    // Объект с текстом – конец игры
    public GameObject endGameMessage;

    // Объект яблочко
    public GameObject apple;

    // Положение первого блока
    private Vector3 firstPitonPartPosition;

    // Start is called before the first frame update
    void Start()
    {
        // Скрываем сообщение о конце игры
        endGameMessage.SetActive(false);
        // устанавливаем значение следующего движения на начальное
        fallTime = fallSetupTime;

        // Сохраняем начальное положение головы
        firstPitonPartPosition = transform.Find("Piton Part (1)").position;
        // Генерируем части питона (клонируем префабы) на координатах головы до количества, заданного начальной переменной
        for (int i = 0; i < pitonPartStartCount; i++) Instantiate(pitonPart, firstPitonPartPosition, Quaternion.identity, transform);

        // Проходимся по всем элементам змеи
        // Чтобы раскрасить голову и текущий хвост
        // Для старта это опциональное действие (для красоты)
        int isLast = transform.childCount; // длина змеии
        bool head = true; // Входим на элемент - голову
        foreach (Transform children in transform)
        {
            if (head) // Если голова
            {
                head = false; // дальше все элементы не голова
                // Меняем цвет головы
                children.GetComponent<SpriteRenderer>().color = new Color32(177, 196, 130, 255);
            }
            // У хвоста меняем цвет для красоты
            if (--isLast == 0) children.GetComponent<SpriteRenderer>().color = new Color32(77, 196, 30, 255);
        }
        // Передвигаем яблоко на новую случайную позицию
        apple.transform.position = new Vector3(UnityEngine.Random.Range(0, MovePiton.width), UnityEngine.Random.Range(0, MovePiton.height));
        // Включаем отрисовку
        apple.GetComponent<SpriteRenderer>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Если игра окончена
        if (endGame)
            // Если нажат пробел - перезапускаем игру
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Снимаем флаг окончания игры
                endGame = false;
                // Устанавливаем направление движения вниз
                pitonMoveDirection = Direction.Down;
                // Нужно удалить все объекты кроме головы
                bool head = true;
                foreach (Transform children in transform)
                {
                    if (head)
                    {
                        head = false;
                        // У головы сбрасываем положение
                        children.position = firstPitonPartPosition;
                    }
                    else
                    {
                        Destroy(children.gameObject);
                    }
                }
                // Повторно запускаем начальную функцию, в ней есть почти все для инициализации игры с нуля
                Start();
                // На этом кадре стоит завершить обработку
                // Иначе будут глюки
                // выходим из функции Update
                return;
            }
            else return;

        // Если нажата одна из кнопок направления
        if (Input.GetKeyDown(KeyCode.UpArrow)) pitonMoveDirection = Direction.Up;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) pitonMoveDirection = Direction.Left;
        if (Input.GetKeyDown(KeyCode.DownArrow)) pitonMoveDirection = Direction.Down;
        if (Input.GetKeyDown(KeyCode.RightArrow)) pitonMoveDirection = Direction.Right;

        // Пришло время подвинуть змейку
        if (Time.time - previousTime > fallTime)
        {

            bool nyam = false; // голова съела яблоко
            bool head = true; // голова
            int isLast = transform.childCount; // хвост
            Vector3 lastPosition = new Vector3(); // позиция предыдущего блока
            // проходим по всем блокам змеи, для головы и хвоста есть особые действия
            foreach (Transform children in transform)
            {
                // сохраняем текущее положение блока
                Vector3 currentPosition = children.position;
                // Это голова
                if (head)
                {
                    head = false; // все остальные блоки не голова
                    // Двигаем голову в соответствии с направлением
                    switch (pitonMoveDirection)
                    {
                        case Direction.Up:
                            children.position += new Vector3(0, 1);
                            break;
                        case Direction.Left:
                            children.position += new Vector3(-1, 0);
                            break;
                        case Direction.Down:
                            children.position += new Vector3(0, -1);
                            break;
                        case Direction.Right:
                            children.position += new Vector3(1, 0);
                            break;
                    }

                    // Считываем положение головы и округляем до целых
                    int roundedX = Mathf.RoundToInt(children.transform.position.x);
                    int roundedY = Mathf.RoundToInt(children.transform.position.y);

                    // если голова вышла за границы стола
                    // или наткнулся на кусок хвоста, конец игры
                    if (roundedX < 0 || roundedX >= width || roundedY < 0 || roundedY >= height || notValidMove())
                    {
                        // Конец игры
                        endGame = true;
                        // Показываем надпись
                        endGameMessage.SetActive(true);
                        // Меняем цвет головы
                        children.GetComponent<SpriteRenderer>().color = new Color32(177, 6, 13, 255);
                    }

                    // если первый блок наткнулся на яблоко
                    // Проверяем, видно ли яблоко, и совпадает голова и координаты яблока
                    if (apple.GetComponent<SpriteRenderer>().enabled && children.transform.position == apple.transform.position)
                    {
                        nyam = true; // Значит голова съела яблоко!
                    }
                }
                else // Это не голова!
                {
                    // Для не головы все просто: подвинуть блок на место головы или предыдущего блока
                    children.position = lastPosition;
                }
                // нужно сохранить в последнюю позицию положение блока до перемещения
                // т.к. следующий блок должен будет перемещен ровно в эту позицию
                lastPosition = currentPosition;

                // Это хвост
                if (--isLast == 0)
                {
                    if (nyam) // Яблоко было съедено!
                    {
                        nyam = false; // Возвращаем флаг о съеденном яблоке в исходное состояние
                        // Генерируем после хвоста еще один блок
                        Instantiate(pitonPart, children.transform.position, Quaternion.identity, transform);
                        // Выключаем видимость яблока, мы его съели!
                        apple.GetComponent<SpriteRenderer>().enabled = false;
                        // Сокращаем время хода
                        // Уменьшаем время на 10%
                        fallTime *= 0.9f;
                    }
                }
            }
            previousTime = Time.time; // сохраняем текущее время, на котором произошел ход
        }
    }

    // Функция проверки пересечения головы с телом
    // возвращает правду или ложь
    private bool notValidMove()
    {
        // Для головы делать проверку с головой не нужно
        bool isHead = true;
        Transform head = transform; // Тут будет голова
        foreach (Transform children in transform)
        {
            if (isHead) // голову пропускаем
            {
                isHead = false; // все остальные блоки не голова
                head = children;
            }
            // Если голова и блок по одним координатам - питон съел себя
            else if (head.position == children.position) return true;
        }
        return false; // Питон себя не съел
    }
}
