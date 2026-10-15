# Actual local test results

34 executed; 31 passed; 3 failed; 0 skipped. Source: core-qa.trx.

| Test instance | Outcome | Duration |
|---|---|---|
| TC01_AllSignalsMatch_Score100 | Passed | 00:00:00.1984148 |
| TC02_CategoryMismatchOrMissing_ScoreZero (" ") | Passed | 00:00:00.0000414 |
| TC02_CategoryMismatchOrMissing_ScoreZero ("") | Passed | 00:00:00.0112455 |
| TC02_CategoryMismatchOrMissing_ScoreZero ("Wallet") | Passed | 00:00:00.0112084 |
| TC03_CaseAndWhitespace_DoNotChangeExactFields | Passed | 00:00:00.1708827 |
| TC04_DateBoundary_UsesOneDayWindow (-1,100) | Passed | 00:00:00.1820849 |
| TC04_DateBoundary_UsesOneDayWindow (-2,85) | Passed | 00:00:00.1934834 |
| TC04_DateBoundary_UsesOneDayWindow (0,100) | Passed | 00:00:00.1985885 |
| TC04_DateBoundary_UsesOneDayWindow (1,100) | Passed | 00:00:00.1857793 |
| TC04_DateBoundary_UsesOneDayWindow (2,85) | Passed | 00:00:00.1858312 |
| TC05_RecommendationThresholds (0,"Not Recommended") | Passed | 00:00:00.0000038 |
| TC05_RecommendationThresholds (100,"High") | Passed | 00:00:00.0000185 |
| TC05_RecommendationThresholds (39,"Not Recommended") | Passed | 00:00:00.0111661 |
| TC05_RecommendationThresholds (40,"Low") | Passed | 00:00:00.0000135 |
| TC05_RecommendationThresholds (59,"Low") | Passed | 00:00:00.0111964 |
| TC05_RecommendationThresholds (60,"Medium") | Passed | 00:00:00.0000123 |
| TC05_RecommendationThresholds (79,"Medium") | Passed | 00:00:00.0000109 |
| TC05_RecommendationThresholds (80,"High") | Passed | 00:00:00.0000099 |
| TC06_NullReport_ScoreZero (False) | Passed | 00:00:00.0003362 |
| TC06_NullReport_ScoreZero (True) | Passed | 00:00:00.0002264 |
| TC07_NullItem_ScoreZero (False) | Passed | 00:00:00.0002737 |
| TC07_NullItem_ScoreZero (True) | Passed | 00:00:00.0002331 |
| TC08_EmptyDescriptions_NoKeywordBonus | Passed | 00:00:00.0004740 |
| TC09_RepeatedKeywords_DoNotExceed100 | Passed | 00:00:00.1788354 |
| TC10_EmptyCollections_NoMatches | Passed | 00:00:00.0049240 |
| TC11_ClosedReports_Excluded (False) | Passed | 00:00:00.0018552 |
| TC11_ClosedReports_Excluded (True) | Passed | 00:00:00.0019616 |
| TC12_AddLost_AssignsDistinctIdsAndSubmittedStatus | Passed | 00:00:00.0033676 |
| TC13_AddFound_AssignsDistinctIdsAndSubmittedStatus | Passed | 00:00:00.0027360 |
| TC14_UIResults_SortedWithExplanations | Passed | 00:00:00.0038763 |
| TC15_RepeatedMatching_DoesNotMutateReports | Passed | 00:00:00.0016453 |
| TC16_D001_UIAndCore_UseSameCategoryExclusion | Failed | 00:00:00.0307139 |
| TC17_D002_ReturnedFoundItem_IsNotAvailableForMatching | Failed | 00:00:00.0306943 |
| TC18_D003_RecreatedService_RetainsSavedReport | Failed | 00:00:00.0306743 |
