﻿using PrototypeGame2D.Object;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static PrototypeGame2D.Core.OrderState;

namespace PrototypeGame2D.Game
{
    public class FoodManager : MonoBehaviour
    {
        #region Singleton class: FoodManager
        public static FoodManager Instance;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
        }

        #endregion

        [SerializeField] private Text _textSpeedSpawn;

        private List<FoodOrder> _foodOrder;
        private List<FoodInfo> _allFoodResource;
        private int _numberMenuOrder;

        private List<FoodInfoSpaw> _foodForSpawn;

        private bool _haveFoodOrder;
        private bool _refreshFoodResource;

        private FoodInfo _foodInfoTmp;
        private FoodOrder _foodOrderTmp;

        private bool _checking = false;

        private float _timeSacle = 0.5f;
        private int _missingOrder = 0;

        private int _numFoodComplete;
        private float _speedSpawn;

        OrderArea areaOrder;

        public List<FoodInfoSpaw> FoodInfoSpaws
        {
            get { return _foodForSpawn; }
            set { _foodForSpawn = value; }
        }

        public bool haveFoodOrder
        {
            get { return _haveFoodOrder; }
            set { _haveFoodOrder = value; }
        }

        public bool refreshFoodResource
        {
            get { return _refreshFoodResource; }
            set { _refreshFoodResource = value; }
        }

        public List<FoodInfo> AllFoodResource
        {
            get { return _allFoodResource; }
            set { _allFoodResource = value; }
        }

        public int numberMenuOrder
        {
            get { return _numberMenuOrder; }
        }

        public float SpeedSpawn
        {
            get { return _speedSpawn; }
            set { _speedSpawn = value; }
        }

        // Start is called before the first frame update
        void Start()
        {
            _foodOrder = new List<FoodOrder>();
            _allFoodResource = new List<FoodInfo>();
            _foodForSpawn = new List<FoodInfoSpaw>();

            _refreshFoodResource = false;
            _haveFoodOrder = false;

            _missingOrder = 0;
            _numFoodComplete = 0;
            _speedSpawn = 0.5f;

            _foodInfoTmp = new FoodInfo();
            _foodOrderTmp = new FoodOrder();

            areaOrder = FindObjectOfType<OrderArea>();
        }

        // Update is called once per frame
        void Update()
        {
            if(_foodOrder.Count > 0 && !GameManager.Instance.isGameOver)
            {
                for(int i = 0; i < _foodOrder.Count; i++)
                {
                    if(_foodOrder[i].statusOrder == STATUS.FOOD_NOT_COMPLETE)
                    {
                        //Debug.Log("orderTime: " + _foodOrder[i].timeOrder);
                        if (_foodOrder[i].timeOrder > 0)
                        {
                            _foodOrder[i].CountDownTime(Time.deltaTime * _timeSacle);
                        }
                        else
                        {
                            _foodOrder[i].timeOrder = 0;
                            _foodOrder[i].statusOrder = STATUS.FOOD_MISSING;
                        }
                        areaOrder.UpdateProgressBar();
                    }
                }

                _foodOrder = _foodOrder.OrderBy(item => item.timeOrder).ToList();
            }

            if(!GameManager.Instance.isGameOver && _haveFoodOrder && !_checking)
            {
                _checking = true;
                FoodSpawn.Instance.StartSpawnFood();
            }

            _textSpeedSpawn.text = "Speed: " + _speedSpawn.ToString();
        }

        private void InitMenuOrder()
        {

        }

        public void OrderFood(FoodOrder order)
        {
            AddOrder(order);
            _haveFoodOrder = true;
        }

        public void RemoveOrder(FoodOrder foodOrder)
        {
            if(foodOrder.statusOrder == STATUS.FOOD_MISSING)
            {
                ++_missingOrder;
                GameManager.Instance.CheckMissingOrder(_missingOrder);
            }
            _foodOrder.Remove(foodOrder);
        }

        public void HandleFood(string id)
        {
            //Debug.Log("FoodManager: foodResource " + id + " _foodForSpawn size " + _foodForSpawn.Count);
            _refreshFoodResource = true;
            //Debug.Log("FoodManager: _foodForSpawn size " + _foodForSpawn.Count);

            float minTime = 1000;

            foreach (FoodOrder fo in _foodOrder)
            {
                if (fo.statusOrder == STATUS.FOOD_NOT_COMPLETE)
                {
                    var fi = fo.foodResource.Where(i => i.id == id).FirstOrDefault();
                    if (fo.timeOrder < minTime)
                    {
                        if(fi.Amount > 0)
                        {
                            Debug.Log("FoodManger: " + fo.id + " line 144");
                            minTime = fo.timeOrder;
                            _foodOrderTmp = fo;
                            _foodInfoTmp = fi;
                        }
                        else
                        {
                            minTime = 1000;
                        }
                    }
                }
            }
            _foodOrderTmp.haveUpdate = true;
            if (_foodInfoTmp.Amount > 0)
            {
                _foodInfoTmp.Amount -= 1;
                for (int i = 0; i < _foodForSpawn.Count; i++)
                {
                    if (_foodForSpawn[i].ID.Equals(_foodInfoTmp.id))
                    {
                        _foodForSpawn.RemoveAt(i);
                        break;
                    }
                }
                _foodOrderTmp.CompletePartProgressOrder();
            }

            UpdateSlotOrder(_foodOrderTmp);
        }

        public void UpdateSlotOrder(FoodOrder order)
        {
            //OrderArea areaOrder = FindObjectOfType<OrderArea>();
            areaOrder.UpdateProgress(order);
        }

        public void AddOrder(FoodOrder foodOrder)
        {
            _foodOrder.Add(foodOrder);

            foreach(FoodInfo fi in foodOrder.foodResource)
            {
                for(int i = 0; i < fi.Amount; i++)
                {
                    FoodInfoSpaw fis = new FoodInfoSpaw();
                    fis.SetFoodSpawn(fi.id, fi.image, fi.SymbolKey);
                    _foodForSpawn.Add(fis);
                }
            }
        }

        public void CompleteOrder(FoodOrder order)
        {
            for(int i = 0; i < _foodOrder.Count; i++)
            {
                if(_foodOrder[i].Name.Equals(order.Name))
                {
                    _foodOrder.RemoveAt(i);
                    ++_numFoodComplete;
                    if (_numFoodComplete % 5 == 0 && _numFoodComplete > 0)
                    {
                        _speedSpawn += 0.05f;
                    }
                    Debug.Log("FoodManager: RemoveOrder " + _numFoodComplete);
                    break;
                }
            }
        }

        public int GetNumberOrder()
        {
            return _foodOrder.Count;
        }
    }
}

