# PR-3 response

### P2-1
- Disposition: full
- Evidence: `ReadLine` of `IronAbsolution.Tools/SteCheck/WritingRules.cs` returned at a heading, a table row, and a fence with no end of the paragraph. A list item flushed the held lines, but it kept the sentence count. The fix ends the paragraph at each of the four blocks.
- Correction: `cbc4689ed492752166b2c17f8575cb16b6344c29`
- Regression check: `SteCheckWritingRuleTests.ABlockEndsTheParagraphBeforeIt` has one case for each block: a heading, a bullet item, a numbered item, a table row, and a fence. Each case failed on the old code. `SevenSentencesWithNoBlockBetweenThemStayOneParagraph` shows that the rule still finds a long paragraph.
