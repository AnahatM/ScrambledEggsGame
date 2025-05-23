using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuyScreenCarSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [SerializeField] private Color redColor = new(255 / 255f, 123 / 255f, 111 / 255f);
    [SerializeField] private Color greenColor = new(154 / 255f, 255 / 255f, 124 / 255f);

    public enum SlotType
    {
        Roster,
        CarShop,
        Scrapyard
    }

    public SlotType slotType;

    [SerializeField] private TextMeshProUGUI priceText;

    public GameObject tempSellVehicle;

    [SerializeField] public BuyScreenItemInfoBtn correspondingInfoBtn;
    [SerializeField] public GameObject sellSlot;

    public void OnDrop(PointerEventData eventData)
    {
        FindObjectOfType<BuyScreenManager>().itemPurchased = true;
        Debug.Log("OnDrop");
        if (eventData.pointerDrag != null)
        {
            BuyScreenCar car = null;
            BuyScreenUltimate ultimate = null;
            tempSellVehicle = null;

            if (eventData.pointerDrag.TryGetComponent<BuyScreenCar>(out car) || eventData.pointerDrag.TryGetComponent<BuyScreenUltimate>(out ultimate)) //eventData.pointerDrag = The car being held by the mouse
            {
                
                //If the slot is not empty
                if ((transform.GetComponentInChildren<BuyScreenCar>() != null) || (transform.GetComponentInChildren<BuyScreenUltimate>() != null))
                {

                    // If the car came from the shop
                    if (eventData.pointerDrag.GetComponent<DragDrop>().startingParent.GetComponent<BuyScreenCarSlot>().slotType == SlotType.CarShop) 
                    {
                        // If the player has enough money to buy the car
                        if ((car != null && BuyScreenManager.instance.CheckMoneyAmount(car.correspondingCar.carShopPrice)) || (ultimate != null && BuyScreenManager.instance.CheckMoneyAmount(ultimate.correspondingUltimate.ultimateShopPrice)))
                        {
                            if (slotType == SlotType.Roster)
                            {
                                if (gameObject.name == "Ultimate Slot" && ultimate != null)
                                {
                                    SendOldItemToSellSlot(transform.GetComponentInChildren<BuyScreenUltimate>());
                                     
                                    // ultimate.gameObject.transform.parent = transform;
                                    ultimate.gameObject.transform.SetParent(transform);
                                    ultimate.gameObject.transform.SetSiblingIndex(1);
                                    ultimate.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                                    ultimate.GetComponent<DragDrop>().canBePlaced = true;
                                    ultimate.GetComponent<DragDrop>().startingParent = transform;

                                    // Take away any money
                                    BuyScreenManager.instance.RemoveMoney(ultimate.correspondingUltimate.ultimateShopPrice);

                                    ultimate.EnablePurchaseParticles();

                                    if(correspondingInfoBtn != null){
                                        // Debug.Log("Droped on rosterslot");
                                        correspondingInfoBtn.ActiveInfo(null, ultimate);
                                    }

                                }
                                else if (gameObject.name != "Ultimate Slot" && car != null)
                                {
                                    SendOldItemToSellSlot(transform.GetComponentInChildren<BuyScreenCar>());

                                    // car.gameObject.transform.parent = transform;
                                    car.gameObject.transform.SetParent(transform);
                                    car.gameObject.transform.SetSiblingIndex(1);
                                    car.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                                    car.GetComponent<DragDrop>().canBePlaced = true;
                                    car.GetComponent<DragDrop>().startingParent = transform;

                                    // Take away any money
                                    BuyScreenManager.instance.RemoveMoney(car.correspondingCar.carShopPrice);
                                    car.EnablePurchaseParticles();

                                    if(correspondingInfoBtn != null){
                                        // Debug.Log("Droped on rosterslot");
                                        correspondingInfoBtn.ActiveInfo(car, null);
                                    }
                                }

                                // Sells the vehicle being replaced
                                // GameObject oldItem = eventData.pointerEnter.GetComponentInChildren<DragDrop>().gameObject;
                                // SellVehicle(oldItem);
                            }
                        }

                    }
                    else
                    {
                        if (eventData.pointerDrag.GetComponent<DragDrop>().startingParent.GetComponent<BuyScreenCarSlot>().slotType == SlotType.Roster && eventData.pointerEnter.GetComponentInChildren<DragDrop>().startingParent.GetComponent<BuyScreenCarSlot>().slotType == SlotType.Roster) // If both cars are currently in the roster
                        {

                            DragDrop heldCar = eventData.pointerDrag.gameObject.GetComponentInChildren<DragDrop>();
                            DragDrop replacedCar = eventData.pointerEnter.gameObject.GetComponentInChildren<DragDrop>();

                            GameObject oldParent = heldCar.startingParent.gameObject;
                            GameObject newParent = replacedCar.startingParent.gameObject;

                            //Takes the pre-existing item and moves it to where your new car used to be 
                            replacedCar.transform.SetParent(oldParent.transform);
                            replacedCar.transform.SetSiblingIndex(1);
                            replacedCar.gameObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                            //Takes your held car and moves it to where the old car used to be
                            heldCar.transform.SetParent(gameObject.transform);
                            heldCar.transform.SetSiblingIndex(1);
                            heldCar.gameObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                            heldCar.GetComponent<DragDrop>().canBePlaced = true;

                            oldParent.GetComponent<BuyScreenCarSlot>().correspondingInfoBtn.ActiveInfo(replacedCar.GetComponent<BuyScreenCar>(), null);
                            newParent.GetComponent<BuyScreenCarSlot>().correspondingInfoBtn.ActiveInfo(heldCar.GetComponent<BuyScreenCar>(), null);

                        }

                    }
                }
                else
                {
                    if (eventData.pointerDrag.GetComponent<DragDrop>().startingParent.GetComponent<BuyScreenCarSlot>().slotType == SlotType.CarShop)
                    {
                        if ((car != null && BuyScreenManager.instance.CheckMoneyAmount(car.correspondingCar.carShopPrice)) || (ultimate != null && BuyScreenManager.instance.CheckMoneyAmount(ultimate.correspondingUltimate.ultimateShopPrice))) // If the player has enough money to buy the car
                        {

                            if (slotType == SlotType.Roster)
                            {
                                if (gameObject.name == "Ultimate Slot" && ultimate != null)
                                {
                                    if(correspondingInfoBtn != null){
                                        // Debug.Log("Droped on rosterslot");
                                        correspondingInfoBtn.ActiveInfo(null, ultimate);
                                    }

                                    // ultimate.gameObject.transform.parent = transform;
                                    ultimate.gameObject.transform.SetParent(transform);
                                    ultimate.gameObject.transform.SetSiblingIndex(1);
                                    ultimate.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                                    ultimate.GetComponent<DragDrop>().canBePlaced = true;
                                    ultimate.GetComponent<DragDrop>().startingParent = transform;


                                    // Take away any money
                                    BuyScreenManager.instance.RemoveMoney(ultimate.correspondingUltimate.ultimateShopPrice);

                                    ultimate.EnablePurchaseParticles();
                                }
                                else if (gameObject.name != "Ultimate Slot" && car != null)
                                {
                                    if(correspondingInfoBtn != null){
                                        // Debug.Log("Droped on rosterslot");
                                        correspondingInfoBtn.ActiveInfo(car, null);
                                    }

                                    // car.gameObject.transform.parent = transform;
                                    car.gameObject.transform.SetParent(transform);
                                    car.gameObject.transform.SetSiblingIndex(1);
                                    car.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                                    car.GetComponent<DragDrop>().canBePlaced = true;
                                    car.GetComponent<DragDrop>().startingParent = transform;

                                    // Take away any money
                                    if (car != null)
                                    {
                                        BuyScreenManager.instance.RemoveMoney(car.correspondingCar.carShopPrice);
                                        car.EnablePurchaseParticles();
                                    }
                                }

                                // NEW: HIDE THE LABEL TEXT (Car/Ult)
                                GetComponentInChildren<TextMeshProUGUI>(true).gameObject.SetActive(false);
                            }

                        }
                        else
                        {
                            eventData.pointerDrag.gameObject.transform.parent = eventData.pointerDrag.GetComponent<DragDrop>().startingParent;
                            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                            eventData.pointerDrag.GetComponent<DragDrop>().canBePlaced = true;
                        }

                    }
                    else if (eventData.pointerDrag.GetComponent<DragDrop>().startingParent.GetComponent<BuyScreenCarSlot>().slotType == BuyScreenCarSlot.SlotType.Roster)
                    {
                        if (slotType == SlotType.Scrapyard)
                        {
                            DecideOnSell(eventData.pointerDrag);
                        }
                        else if (slotType == SlotType.Roster)
                        {
                            //Clear the other's info button
                            if (eventData.pointerDrag.GetComponent<DragDrop>().startingParent.GetComponent<BuyScreenCarSlot>().correspondingInfoBtn != null)
                            {

                                eventData.pointerDrag.GetComponent<DragDrop>().startingParent.GetComponent<BuyScreenCarSlot>().correspondingInfoBtn.DisableInfo();
                            }

                            //If the vehicle is moving from one roster slot to an empty slot
                            eventData.pointerDrag.gameObject.transform.parent = transform;
                            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                            eventData.pointerDrag.GetComponent<DragDrop>().canBePlaced = true;
                            eventData.pointerDrag.gameObject.transform.SetSiblingIndex(1);

                            if (correspondingInfoBtn != null)
                            {
                                // Debug.Log("Droped on rosterslot");
                                correspondingInfoBtn.ActiveInfo(car, null);
                            }
                        }
                        else
                        {
                            eventData.pointerDrag.gameObject.transform.parent = eventData.pointerDrag.GetComponent<DragDrop>().startingParent;
                            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                            eventData.pointerDrag.GetComponent<DragDrop>().canBePlaced = true;
                        }

                    }


                }
            }






            //eventData.pointerDrag.gameObject.transform.parent = transform;
        }
        else
        {
            eventData.pointerDrag.GetComponent<DragDrop>().canBePlaced = false;
        }

    }

    private void SendOldItemToSellSlot(BuyScreenCar sellCar){
        //if sell slot os already populated, sell car in sell slot
        if(sellSlot.GetComponent<BuyScreenCarSlot>().tempSellVehicle != null){
            sellSlot.GetComponent<BuyScreenCarSlot>().SellVehicle();
        }

        sellCar.GetComponent<DragDrop>().emergencyParent = sellCar.GetComponent<DragDrop>().startingParent;

        sellCar.gameObject.transform.SetParent(sellSlot.transform);
        sellCar.gameObject.transform.SetSiblingIndex(1);
        sellCar.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        sellCar.GetComponent<DragDrop>().canBePlaced = true;
        sellCar.GetComponent<DragDrop>().startingParent = sellSlot.transform;
        // DecideOnSell(oldCar.gameObject);

        sellSlot.GetComponent<BuyScreenCarSlot>().tempSellVehicle = sellCar.gameObject;
        // BuyScreenManager.instance.SellPopup();
        sellSlot.GetComponent<BuyScreenCarSlot>().AcceptSell();
    }

    private void SendOldItemToSellSlot(BuyScreenUltimate sellUlt){
        //if sell slot os already populated, sell car in sell slot
        if(sellSlot.GetComponent<BuyScreenCarSlot>().tempSellVehicle != null){
            sellSlot.GetComponent<BuyScreenCarSlot>().SellVehicle();
        }

        sellUlt.GetComponent<DragDrop>().emergencyParent = sellUlt.GetComponent<DragDrop>().startingParent;

        sellUlt.gameObject.transform.SetParent(sellSlot.transform);
        sellUlt.gameObject.transform.SetSiblingIndex(1);
        sellUlt.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        sellUlt.GetComponent<DragDrop>().canBePlaced = true;
        sellUlt.GetComponent<DragDrop>().startingParent = sellSlot.transform;
        // DecideOnSell(oldUlt.gameObject);

        sellSlot.GetComponent<BuyScreenCarSlot>().tempSellVehicle = sellUlt.gameObject;
        // BuyScreenManager.instance.SellPopup();
        sellSlot.GetComponent<BuyScreenCarSlot>().AcceptSell();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //When the player clicks on the item slot, the info box will be filled out with the corresponding information

        if (eventData.pointerClick != null)
        {
            GameObject shopItem = null;

            // Check if pointerDrag is not null before accessing its gameObject
            if (eventData.pointerDrag != null)
            {
                shopItem = eventData.pointerDrag.gameObject;
            }

            if (shopItem != null)
            {
                // Check if it's a right-click (secondary button)
                if (eventData.button == PointerEventData.InputButton.Right)
                {
                    Button button = shopItem.transform.parent.GetComponentInChildren<Button>();
                    if (button != null)
                    {
                        button.onClick.Invoke(); // Invoke the button's onClick event
                    }
                }
            }
        }
    }

    public void DecideOnSell(GameObject vehicle)
    {
        vehicle.GetComponent<DragDrop>().emergencyParent = vehicle.GetComponent<DragDrop>().startingParent;

        vehicle.gameObject.transform.parent = transform;
        vehicle.gameObject.transform.SetSiblingIndex(1);
        vehicle.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        vehicle.GetComponent<DragDrop>().canBePlaced = true;
        vehicle.GetComponent<DragDrop>().startingParent = transform;

        tempSellVehicle = vehicle;

        float sellAmount = 0;
        BuyScreenCar car = null;
        BuyScreenUltimate ultimate = null;

        if (tempSellVehicle.TryGetComponent<BuyScreenCar>(out car) || vehicle.TryGetComponent<BuyScreenUltimate>(out ultimate))
        {
            if (car != null)
            {
                sellAmount = car.correspondingCar.carShopPrice / 2;
            }
            else if (ultimate != null)
            {
                sellAmount = 25f;
            }
        }

        BuyScreenManager.instance.SellButtonOpen(sellAmount);
    }

    public void SellVehicle(GameObject vehicle = null)
    {
        if (tempSellVehicle != null)
        {
            vehicle = tempSellVehicle;
        }

        BuyScreenCar car = null;
        BuyScreenUltimate ultimate = null;

        int sellAmount = 0;


        if (vehicle.TryGetComponent<BuyScreenCar>(out car) || vehicle.TryGetComponent<BuyScreenUltimate>(out ultimate))
        {
            if (ultimate != null)
            {
                // ultimate.gameObject.transform.parent = transform;
                ultimate.gameObject.transform.SetParent(transform);
                ultimate.gameObject.transform.SetSiblingIndex(1);
                ultimate.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                ultimate.GetComponent<DragDrop>().canBePlaced = true;

                sellAmount = 25;
                // Add Money
                BuyScreenManager.instance.AddMoney(sellAmount);

                ultimate.EnableSellParticles();

                ultimate.gameObject.GetComponent<Animator>().enabled = true;

                ultimate.gameObject.GetComponent<Animator>().Play("SellShrink");

                //DESTROY THE VEHICLE
                Destroy(ultimate.gameObject, 0.6f);
            }
            else if (car != null)
            {
                // car.gameObject.transform.parent = transform;
                car.gameObject.transform.SetParent(transform);
                car.gameObject.transform.SetSiblingIndex(1);
                car.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                car.GetComponent<DragDrop>().canBePlaced = true;

                // Add Money
                
                sellAmount = Mathf.FloorToInt(car.correspondingCar.carShopPrice / 2);

                BuyScreenManager.instance.AddMoney(sellAmount);

                if (car.GetComponent<DragDrop>().startingParent.GetComponent<BuyScreenCarSlot>().slotType == BuyScreenCarSlot.SlotType.Scrapyard)
                {
                    car.EnableSellParticles();

                    car.gameObject.GetComponent<Animator>().enabled = true;

                    car.gameObject.GetComponent<Animator>().Play("SellShrink");

                    //DESTROY THE VEHICLE
                    Destroy(car.gameObject, 0.6f);
                }
                else
                {
                    //DESTROY THE VEHICLE
                    Destroy(car.gameObject);
                }

                
            }
        }

        BuyScreenManager.instance.SellButtonClose();

        BuyScreenManager.instance.SellAmountHidePopup(); //Resets animation, necessary
        BuyScreenManager.instance.SellAmountShowPopup(sellAmount);

    }

    public void AcceptSell()
    {
        SellVehicle(tempSellVehicle);
        GameObject obj = tempSellVehicle.GetComponent<DragDrop>().emergencyParent.gameObject;
        obj.GetComponent<BuyScreenCarSlot>().correspondingInfoBtn.DisableInfo();
    }

    public void DeclineSell()
    {
        Transform originalParent = tempSellVehicle.GetComponent<DragDrop>().emergencyParent;
        if(originalParent.GetComponentInChildren<BuyScreenUltimate>() == null && originalParent.GetComponentInChildren<BuyScreenCar>() == null){
            tempSellVehicle.gameObject.transform.parent = originalParent;
            // tempSellVehicle.gameObject.transform.parent = tempSellVehicle.GetComponent<DragDrop>().emergencyParent;
            tempSellVehicle.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            tempSellVehicle.GetComponent<DragDrop>().canBePlaced = true;

            BuyScreenManager.instance.SellButtonClose();
        }
        else{
            Debug.Log("You have populated the slot!");
        }

    }

    public void UpdateBGColour()
    {
        if (slotType == SlotType.CarShop)
        {
            BuyScreenCar car = transform.GetComponentInChildren<BuyScreenCar>();
            BuyScreenUltimate ultimate = transform.GetComponentInChildren<BuyScreenUltimate>();

            if (car != null)
            {
                if (BuyScreenManager.instance.CheckMoneyAmount(car.correspondingCar.carShopPrice)) //if the player has enough money to buy the car)
                {
                    priceText.color = greenColor;
                }
                else
                {
                    priceText.color = redColor;
                }
            }
            else if (ultimate != null)
            {
                if (BuyScreenManager.instance.CheckMoneyAmount(ultimate.correspondingUltimate.ultimateShopPrice)) //if the player has enough money to buy the car)
                {
                    priceText.color = greenColor;
                }
                else
                {
                    priceText.color = redColor;
                }
            }


        }
    }

    public void UpdatePriceText()
    {
        BuyScreenCar car = transform.GetComponentInChildren<BuyScreenCar>();
        BuyScreenUltimate ultimate = transform.GetComponentInChildren<BuyScreenUltimate>();

        if (car != null)
        {
            priceText.text = "$" + car.correspondingCar.carShopPrice.ToString("0");
        }
        else if (ultimate != null)
        {
            priceText.text = "$" + ultimate.correspondingUltimate.ultimateShopPrice.ToString("0");
        }

        UpdateBGColour();


    }
}
