using UnityEngine;

namespace BaseClass
{
    public class Arrow : MonoBehaviour
    {
        Rigidbody2D rigidbody2d;
        private bool isPlayerArrow;
        [SerializeField] private Sprite[] arrowSprite;
        Troop sourcerTroop;

    
        public void Launch(byte direction, bool fireArrow,Troop attacker)
        {
            Vector2 v = new Vector2();
            switch (direction)
            {
                case 0:
                    v = Vector2.up;
                    break;
                case 1:
                    v = Vector2.down;
                    break;
                case 2:
                    v = Vector2.left;
                    break;
                case 3:
                    v = Vector2.right;
                    break;
            }
            gameObject.GetComponent<SpriteRenderer>().sprite = fireArrow ? arrowSprite[0] : arrowSprite[1];
            rigidbody2d = gameObject.GetComponent<Rigidbody2D>();
            sourcerTroop = attacker;
            isPlayerArrow = attacker.isPlayer;
            rigidbody2d.linearVelocity = v * 1000f;
        }
    
        void OnCollisionEnter2D(Collision2D other)
        {
            if (isPlayerArrow && other.gameObject.layer == LayerMask.NameToLayer("AI"))
            {
                Troop troop = other.gameObject.GetComponent<Troop>();
                if (troop != null)
                {
                    troop.TakeDamage(sourcerTroop);
                    Destroy(gameObject);
                }
            }
            else if (!isPlayerArrow && other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Troop troop = other.gameObject.GetComponent<Troop>();
                if (troop != null)
                {
                    troop.TakeDamage(sourcerTroop);
                    Destroy(gameObject);
                }
            }
        }
    }
}
