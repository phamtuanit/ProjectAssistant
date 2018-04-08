//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ContentManagement.cs" company="SMEE">
//    Copyright (c) SMEE, 2016
//    All rights are reserved. Reproduction or transmission in whole or in part, in
//    any form or by any means, electronic, mechanical or otherwise, is prohibited
//    without the prior written consent of the copyright owner.
//  </copyright>
//  <summary>
//    Defines the ContentManagement.cs type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------
namespace ProjectAssistant.App.Interface
{
    using Caliburn.Micro;

    public interface IContentManagement
    {
        /// <summary>
        /// Shows the content.
        /// </summary>
        /// <param name="content">The content.</param>
        void ShowContent(Screen content);
    }
}