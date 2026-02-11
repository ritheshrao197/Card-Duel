using UnityEngine;

namespace CardDuel.Utils
{
    public static class ValidationUtils
    {
        /// <summary>
        /// Validates that all required UI elements are assigned
        /// </summary>
        /// <param name="uiElements">Array of UI elements to validate</param>
        /// <param name="contextObjectName">Name of the GameObject for error messages</param>
        /// <returns>True if all elements are assigned, false otherwise</returns>
        public static bool ValidateUIElements(Object[] uiElements, string contextObjectName)
        {
            foreach (Object element in uiElements)
            {
                if (element == null)
                {
                    Debug.LogError("One or more UI elements are not assigned in " + contextObjectName);
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// Validates that a CardData object is not null
        /// </summary>
        /// <param name="cardData">CardData to validate</param>
        /// <param name="contextObjectName">Name of the GameObject for error messages</param>
        /// <returns>True if card data is valid, false otherwise</returns>
        public static bool ValidateCardData(object cardData, string contextObjectName)
        {
            if (cardData == null)
            {
                Debug.LogError("CardData is null when binding to " + contextObjectName);
                return false;
            }
            return true;
        }
    }
}