﻿using System;

namespace WLO_Translator_WPF
{
    public class FoundItemDataCollection
    {
        private ItemData            mItem;
        private byte[]              mBytes;
        private FileItemProperties  mFileItemProperties;

        private int                 mItemExtra1LengthPosition;
        private int                 mItemExtra2Length;

        public DataCollectionStates DataCollectionState     { get; private set; }
        public int                  ItemDescriptionLength   { get; private set; }

        public enum DataCollectionStates
        {
            NAME_LENGTH,
            NAME_AND_ID,
            DESCRIPTION_LENGTH,
            DESCRIPTION,
            EXTRA1_LENGTH,
            EXTRA1,
            EXTRA2_LENGTH,
            EXTRA2
        }

        public FoundItemDataCollection(ref byte[] bytes, FileItemProperties fileItemProperties)
        {
            mItem               = new ItemData();
            mBytes              = bytes;
            mFileItemProperties = fileItemProperties;
            DataCollectionState = DataCollectionStates.NAME_LENGTH;
        }

        public ItemData CollectData(int startIndex, int endIndex, int idOffset)
        {
            int itemStartPosition = -1, itemExtra1LengthPosition = -1, itemExtra2LengthPosition = -1;
            for (int i = startIndex; i < endIndex; ++i)
            {
                byte currentByte = mBytes[i];

                // Find start of item
                if (currentByte != '\0')
                {
                    //zeroCounter = 0;

                    if (DataCollectionState == DataCollectionStates.NAME_LENGTH)
                    {
                        itemStartPosition = i;

                        CollectNameLength(i);
                    }
                    else if (DataCollectionState == DataCollectionStates.NAME_AND_ID)
                    {
                        int itemNameLength = mBytes[itemStartPosition];

                        // Break if outside boundaries
                        if (i + itemNameLength >= mBytes.Length) { return null; }

                        ItemData item = CollectFindNameAndID(ref i, idOffset);
                        if (item != null)
                            return item;
                    }
                    else if (DataCollectionState == DataCollectionStates.DESCRIPTION_LENGTH)
                    {
                        CollectDescriptionLength(ref i);
                    }
                    else if (DataCollectionState == DataCollectionStates.DESCRIPTION)
                    {
                        // Break if outside boundaries
                        if (i + ItemDescriptionLength >= mBytes.Length) { return null; }

                        ItemData item = CollectDescription(ref i);
                        if (item != null)
                            return item;
                    }
                    else if (DataCollectionState == DataCollectionStates.EXTRA1_LENGTH)
                    {
                        itemExtra1LengthPosition = i;
                        CollectExtra1Length(i);
                    }
                    else if (DataCollectionState == DataCollectionStates.EXTRA1)
                    {
                        int itemExtra1Length = mBytes[itemExtra1LengthPosition];

                        // Break if outside boundaries
                        if (i + itemExtra1Length >= mBytes.Length) { return null; }

                        CollectExtra1(ref i);
                    }
                    else if (DataCollectionState == DataCollectionStates.EXTRA2_LENGTH)
                    {
                        itemExtra2LengthPosition = i;
                        CollectExtra2Length(ref i);
                    }
                    else if (DataCollectionState == DataCollectionStates.EXTRA2)
                    {
                        int itemExtra2Length = mBytes[itemExtra2LengthPosition];

                        // Break if outside boundaries
                        if (i + itemExtra2Length >= mBytes.Length)
                        {
                            return null;
                        }

                        ItemData item = CollectExtra2(ref i);
                        if (item != null)
                            return item;
                    }
                }
            }

            return null;
        }

        public void CollectNameLength(int i)
        {
            if (DataCollectionState != DataCollectionStates.NAME_LENGTH)
                throw new ArgumentException("ERROR: Collection State Not Equal to NAME_LENGTH");

            mItem.ItemStartPosition = i;

            DataCollectionState = DataCollectionStates.NAME_AND_ID;
        }

        public ItemData CollectFindNameAndID(ref int index, int idOffset)
        {
            if (DataCollectionState != DataCollectionStates.NAME_AND_ID)
                throw new ArgumentException("ERROR: Collection State Not Equal to NAME_AND_ID");

            int itemNameStartPosition = index;
            int itemNameLength = mBytes[mItem.ItemStartPosition];

            StoreDataIntoItem(ref mItem, itemNameStartPosition, itemNameLength, DataCollectionState,
                mFileItemProperties.DataFromRightToLeft);
            StoreIDDataIntoItem(ref mItem, idOffset);

            // Decide if description should be collected of not
            if (mFileItemProperties.HasDescription)
            {
                index += itemNameLength;

                DataCollectionState = DataCollectionStates.DESCRIPTION_LENGTH;
                return null;
            }
            else
            {
                index = mItem.ItemStartPosition + mFileItemProperties.LengthPerItem - 1;

                if (index < mBytes.Length)
                    mItem.ItemEndPosition = index;
                else
                    mItem.ItemEndPosition = mBytes.Length - 1;

                DataCollectionState = DataCollectionStates.NAME_LENGTH;
                return mItem;
            }
        }

        public void CollectDescriptionLength(ref int index)
        {
            if (DataCollectionState != DataCollectionStates.DESCRIPTION_LENGTH)
                throw new ArgumentException("ERROR: Collection State Not Equal to DESCRIPTION_LENGTH");

            index += mFileItemProperties.AfterNameToDescriptionLength - 1;
            ItemDescriptionLength = mBytes[index];

            // If the description length becomes 0, decrease i by 1 and get the new description length from that position
            while (ItemDescriptionLength == 0 && index > mItem.IDEndPosition)
            {
                --index;
                ItemDescriptionLength = mBytes[index];

                Console.WriteLine("WARNING: Item \"" + mItem.Name + "\"'s description length was 0 and is now of length "
                    + ItemDescriptionLength.ToString());
            }

            mItem.DescriptionLengthPosition = index;
            DataCollectionState = DataCollectionStates.DESCRIPTION;
        }

        public ItemData CollectDescription(ref int index)
        {
            if (DataCollectionState != DataCollectionStates.DESCRIPTION)
                throw new ArgumentException("ERROR: Collection State Not Equal to DESCRIPTION");

            int itemDescriptionStartPosition = index;

            StoreDataIntoItem(ref mItem, itemDescriptionStartPosition, ItemDescriptionLength,
                DataCollectionState, mFileItemProperties.DataFromRightToLeft);

            index = mItem.ItemStartPosition + mFileItemProperties.LengthPerItem - 1;

            if (mFileItemProperties.FileType != FileType.MARK)
            {
                DataCollectionState = DataCollectionStates.NAME_LENGTH;
                mItem.ItemEndPosition = index;
                return mItem;
            }
            else
            {
                DataCollectionState = DataCollectionStates.EXTRA1_LENGTH;
                return null;
            }
        }

        public void CollectExtra1Length(int index)
        {
            if (DataCollectionState != DataCollectionStates.EXTRA1_LENGTH)
                throw new ArgumentException("ERROR: Collection State Not Equal to EXTRA1_LENGTH");

            mItemExtra1LengthPosition = index;
            DataCollectionState = DataCollectionStates.EXTRA1;
        }

        public void CollectExtra1(ref int index)
        {
            if (DataCollectionState != DataCollectionStates.EXTRA1)
                throw new ArgumentException("ERROR: Collection State Not Equal to EXTRA1");

            int itemExtra1StartPosition = index;
            int itemExtra1Length = mBytes[mItemExtra1LengthPosition];

            StoreDataIntoItem(ref mItem, itemExtra1StartPosition, itemExtra1Length,
                DataCollectionStates.EXTRA1, mFileItemProperties.DataFromRightToLeft);
            mItem.Extra1LengthPosition = mItemExtra1LengthPosition;

            index += itemExtra1Length;
            DataCollectionState = DataCollectionStates.EXTRA2_LENGTH;
        }

        public void CollectExtra2Length(ref int index)
        {
            if (DataCollectionState != DataCollectionStates.EXTRA2_LENGTH)
                throw new ArgumentException("ERROR: Collection State Not Equal to EXTRA2_LENGTH");

            index += mFileItemProperties.AfterNameToDescriptionLength - 1;
            mItemExtra2Length = mBytes[index];

            // If the description length becomes 0, decrease i by 1 and get the new description length from that position
            while (mItemExtra2Length == 0 && index > mItem.Extra1EndPosition)
            {
                --index;
                mItemExtra2Length = mBytes[index];

                Console.WriteLine("WARNING: Item \"" + mItem.Name + "\"'s extra 2 length was 0 and is now of length "
                    + mItemExtra2Length.ToString());
            }

            mItem.Extra2LengthPosition = index;
            DataCollectionState = DataCollectionStates.EXTRA2;
        }

        public ItemData CollectExtra2(ref int index)
        {
            if (DataCollectionState != DataCollectionStates.EXTRA2)
                throw new ArgumentException("ERROR: Collection State Not Equal to EXTRA2");

            int itemExtra2StartPosition = index;

            StoreDataIntoItem(ref mItem, itemExtra2StartPosition, mItemExtra2Length, DataCollectionState,
                mFileItemProperties.DataFromRightToLeft);

            index = mItem.ItemStartPosition + mFileItemProperties.LengthPerItem * 2 - 1;

            DataCollectionState = DataCollectionStates.NAME_LENGTH;
            mItem.ItemEndPosition = index;
            return mItem;
        }

        /* Store data in item */

        private void StoreDataIntoItem(ref ItemData item, int startPosition, int length,
            DataCollectionStates dataCollectionState, bool dataFromRightToLeft)
        {
            // Get the item data
            string data = "";
            for (int j = startPosition; j < startPosition + length; ++j)
                data += (char)mBytes[j];

            // Clean the item data from bad chars
            data = TextManager.CleanStringFromNewLinesAndBadChars(data);

            switch (dataCollectionState)
            {
                case DataCollectionStates.NAME_AND_ID:
                    if (dataFromRightToLeft)
                        item.NameReversed           = data;
                    else
                        item.Name                   = data;
                    item.NameStartPosition          = startPosition;
                    item.NameEndPosition            = startPosition + length;
                    break;
                case DataCollectionStates.DESCRIPTION:
                    if (dataFromRightToLeft)
                        item.DescriptionReversed    = data;
                    else
                        item.Description            = data;
                    item.DescriptionStartPosition   = startPosition;
                    item.DescriptionEndPosition     = startPosition + length;
                    break;
                case DataCollectionStates.EXTRA1:
                    if (dataFromRightToLeft)
                        item.Extra1Reversed         = data;
                    else
                        item.Extra1                 = data;
                    item.Extra1StartPosition        = startPosition;
                    item.Extra1EndPosition          = startPosition + length;
                    break;
                case DataCollectionStates.EXTRA2:
                    if (dataFromRightToLeft)
                        item.Extra2Reversed         = data;
                    else
                        item.Extra2                 = data;
                    item.Extra2StartPosition        = startPosition;
                    item.Extra2EndPosition          = startPosition + length;
                    break;
            }
        }

        private void StoreIDDataIntoItem(ref ItemData item, int idOffset)
        {
            // Extract the item ID
            int nameEndPositionWithOffset;
            if (mFileItemProperties.FileType == FileType.ACTIONINFO)
                nameEndPositionWithOffset = item.ItemStartPosition  + idOffset;
            else
                nameEndPositionWithOffset = item.NameEndPosition    + idOffset;

            while (mBytes.Length <= nameEndPositionWithOffset + 2)
                nameEndPositionWithOffset--;

            item.ID = new int[]
            {
                mBytes[nameEndPositionWithOffset    ],
                mBytes[nameEndPositionWithOffset + 1],
                mBytes[nameEndPositionWithOffset + 2]
            };
            item.IDStartPosition = nameEndPositionWithOffset;
            item.IDEndPosition   = nameEndPositionWithOffset + 2;
        }
    }
}