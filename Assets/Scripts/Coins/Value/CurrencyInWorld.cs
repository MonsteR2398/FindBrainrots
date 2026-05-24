using System.Collections;
using System.Collections.Generic;
using ModularTreasures.Achievements;
using Treasures.CurrencySystem;
using UnityEngine;
public class CurrencyInWorld : MonoBehaviour
{
    public CurrencyValue currency;
    public PhysObject physObject;
    [SerializeField] private ParticleSystem _pickupParticle;


    private void OnTriggerEnter(Collider other)
    {
        CurrencyService.Instance.AddBalance(currency.Type, currency.Value, true);

        if (!physObject.isSet)
            return;

        //UISound.manager.PlayGetMoney(false);
        switch (currency.Type)
        {
            case CurrencyType.Gold:
                    AchievementManager.Instance.AddProgress("coins_find", 1);
                    AchievementManager.Instance.AddProgress("coins_earn", currency.Value);
            break;
            case CurrencyType.Diamond:
                    AchievementManager.Instance.AddProgress("diamonds_find", 1);
                    AchievementManager.Instance.AddProgress("diamonds_earn", currency.Value);
            break;
        }

        Instantiate(_pickupParticle, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
