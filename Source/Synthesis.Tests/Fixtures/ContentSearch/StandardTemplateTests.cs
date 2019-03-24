﻿using System;
using System.ComponentModel;
using System.Linq;
using FluentAssertions;
using Sitecore;
using Sitecore.ContentSearch.Linq;
using Sitecore.Data;
using Sitecore.Globalization;
using Xunit;

namespace Synthesis.Tests.Fixtures.ContentSearch
{
	/*
	 * ASSUMPTIONS OF THESE TESTS:
	 * Standard sitecore_master_index configured
	 * All items in master are allowed to be indexed
	 * The _system_ templates are relatively unmodified (used as query playground)
	 */
    [Trait("Category", "Content Search Tests")]
    public class StandardTemplateTests : ContentSearchTestFixture
	{
		// /sitecore/templates/System/Security/Security folder
		private readonly ID _singleItemId = new ID("{AAD4C04A-EAA6-4824-87D2-E01F2325D422}");
		private const string SingleItemKeyword = "Security folder";
		// /sitecore/templates/System/Security/Role
		private readonly ID _parentItemId = new ID("{A7DF04B4-4C4B-44B7-BE1E-AD901BD53DAD}");

        [Fact(Skip = "Need to determine how to properly mock ContentSearch")]
        public void ContentSearch_FindsItem_ById()
		{
			using (var context = CreateTestSearchContext())
			{
				var query = context.GetSynthesisQueryable<IStandardTemplateItem>()
					.Where(x => x.Id == _singleItemId);

                query.Count().Should().Be(1, "because the leaf node should match the ID");
			}
		}

		[Fact(Skip = "Need to determine how to properly mock ContentSearch")]
		public void ContentSearch_FindsItem_ByKeywordSearch()
		{
			using (var context = CreateTestSearchContext())
			{
				var query = context.GetSynthesisQueryable<IStandardTemplateItem>()
					.Where(x => x.SearchableContent.Contains(SingleItemKeyword));

				query.Count().Should().Be(1, "because the leaf node item should match the content search keyword");
			}
		}

		[Fact(Skip = "Need to determine how to properly mock ContentSearch")]
		public void ContentSearch_FindsTemplateItem_ByName()
		{
			using (var context = CreateTestSearchContext())
			{
				var query = context.GetSynthesisQueryable<IStandardTemplateItem>()
					.Where(x => x.Name == SingleItemKeyword);

				query.Count().Should().Be(1, "because the leaf node item should be found using content search by Name!");
			}
		}

		[Fact(Skip = "Need to determine how to properly mock ContentSearch")]
		public void ContentSearch_FindsDescendantsAndSelf_ByAncestorIds()
		{
			using (var context = CreateTestSearchContext())
			{
				var query = context.GetSynthesisQueryable<IStandardTemplateItem>()
					.Where(x => x.AncestorIds.Contains(_parentItemId));

				// should find 'Role' parent, and 'Data' and 'Roles' children/grandchildren
				Assert.Equal(3, query.Count());// "Could not find the right number of descendants searching by AncestorIds!");
			}
		}



		[Fact(Skip = "Need to determine how to properly mock ContentSearch")]
		public void ContentSearch_FindsChildren_ByParentId()
		{
			using (var context = CreateTestSearchContext())
			{
				var query = context.GetSynthesisQueryable<IStandardTemplateItem>()
					.Where(x => x.ParentId == _parentItemId)
					.ToArray();

				// should find 'Data' child
				query.Count().Should().Be(1);//, "Could not find the child searching by ParentId!");
				Assert.True(query.First().Name.Equals("Data"), "Found wrong child searching by ParentId!");
			}
		}

		[Fact(Skip = "Need to determine how to properly mock ContentSearch")]
		public void ContentSearch_FindsItem_ByTemplateId()
		{
			using (var context = CreateTestSearchContext())
			{
				var query = context.GetSynthesisQueryable<IStandardTemplateItem>()
					.Where(x => x.TemplateId == TemplateIDs.TemplateField && x.AncestorIds.Contains(_parentItemId))
					.ToArray();

				// should find 'Roles' child
				query.Count().Should().Be(1);//, "Could not find the child searching by TemplateId!");
				Assert.True(query.First().Name.Equals("Roles"), "Found wrong child searching by TemplateId!");
			}
		}

		//[Fact(Skip = "Need to determine how to properly mock ContentSearch")]
		// THIS IS HERE AS AN EXAMPLE OF WHAT NOT TO DO
		// These sorts of queries (using StartsWith()) are both very expensive on the index
		// as well as prone - such as this one - to result in so much query expansion once parsed
		// as to hit the maximum number of query clauses in Lucene. That's 1024 by default. Don't do this.
		// If you need to find 'path starts with', search by AncestorIds.Contains() instead - way faster.
		//public void ContentSearch_FindsItems_ByPath()
		//{
		//	using (var context = CreateTestSearchContext())
		//	{
		//		var query = context.GetSynthesisQueryable<IStandardTemplateItem>()
		//			.Where(x => x.Path.StartsWith("/sitecore/templates"))
		//			.ToArray();

		//		// should find a bunch of stuff
		//		Assert.Greater(query.Length, 0, "Found no results under /sitecore/templates by path!");
		//	}
		//}

		[Fact(Skip = "Need to determine how to properly mock ContentSearch")]
		public void ContentSearch_FindsInheritance_ByTemplateIds()
		{
			using (var context = CreateTestSearchContext())
			{
				// /sitecore/templates/System/Layout/Sections/Rendering Options (inherited by Sublayout)
				var renderingOptionsTemplateId = new ID("{D1592226-3898-4CE2-B190-090FD5F84A4C}");

				var allSublayouts = context.GetSynthesisQueryable<IStandardTemplateItem>()
					.Where(x => x.TemplateId == TemplateIDs.Sublayout)
					.ToArray();

				var allRenderingOptionsSublayouts = context.GetSynthesisQueryable<IStandardTemplateItem>()
					.Where(x => x.TemplateId == TemplateIDs.Sublayout && x.TemplateIds.Contains(renderingOptionsTemplateId))
					.ToArray();

				// should find the same number of items (e.g. all implement their base template)
				Assert.Equal(allSublayouts.Length, allRenderingOptionsSublayouts.Length);//, "Number of base template instances does not match inherited template instances!");
				allSublayouts.Length.Should().BeGreaterThan(0);//, "Found no sublayouts - test inconclusive.");
			}
		}

		[Fact(Skip = "Need to determine how to properly mock ContentSearch")]
		public void ContentSearch_FindsItems_ByDatabase()
		{
			using (var context = CreateTestSearchContext())
			{
				var query = context.GetSynthesisQueryable<IStandardTemplateItem>()
					.Where(x => x.DatabaseName == "master")
					.GetResults();

				query.TotalSearchResults.Should().BeGreaterThan(0);//, "Could not find any items by master database!");
			}
		}

		[Fact(Skip = "Need to determine how to properly mock ContentSearch")]
		public void ContentSearch_FindsItem_ByLanguage()
		{
			using (var context = CreateTestSearchContext())
			{
				var query = context.GetSynthesisQueryable<IStandardTemplateItem>()
					.Where(x => x.Id == _singleItemId && x.Language == Language.Parse("en"));

                query.Count().Should().BeGreaterThan(1);// "Could not find an item by en language!");
			}
		}

		[Fact(Skip = "Need to determine how to properly mock ContentSearch")]
		public void ContentSearch_FindsItem_ByCreatedDateAfter()
		{
			// note: test applies to UpdatedDate as well
			using (var context = CreateTestSearchContext())
			{
				var query = context.GetSynthesisQueryable<IStandardTemplateItem>()
					.Where(x => x.Id == _singleItemId && x.CreatedDate > new DateTime(2000, 1, 1));

				query.Count().Should().Be(1);//, "Could not find an item by created date after!");
			}
		}

		[Fact(Skip = "Need to determine how to properly mock ContentSearch")]
		public void ContentSearch_FindsItem_ByCreatedDateBefore()
		{
			// note: test applies to UpdatedDate as well
			using (var context = CreateTestSearchContext())
			{
				var query = context.GetSynthesisQueryable<IStandardTemplateItem>()
					.Where(x => x.Id == _singleItemId && x.CreatedDate < new DateTime(2040, 1, 1));

                query.Count().Should().Be(1);//"Could not find an item by created date before!");
			}
		}

		[Fact(Skip = "Need to determine how to properly mock ContentSearch")]
		public void ContentSearch_FindsItem_ByCustomSearchFieldIndexer()
		{
			using (var context = CreateTestSearchContext())
			{
				var query = context.GetSynthesisQueryable<IStandardTemplateItem>()
					.Where(x => x.TemplateId == TemplateIDs.Sublayout && x["placeholder"] == "content");

				query.Count().Should().BeGreaterOrEqualTo(1);//, "Could not find a sublayout item with placeholder set to 'content'!");
			}
		}
	}
}
