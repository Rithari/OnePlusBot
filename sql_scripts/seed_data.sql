
INSERT INTO `PersistentData` VALUES 
(1,'server_id', 0, ''),
(3,'rolemanager_message_id',0, ''),
(4,'starboard_stars',1, ''),
(5,'level_2_stars',2, ''),
(6,'level_3_stars',3, ''),
(7,'decay_days', 90, ''),
(8, 'modmail_category_id', 0, ''),
(9, 'user_name_illegal_characters', 0, '');

INSERT INTO `AuthTokens` VALUES 
(1,'stable','REPLACE WITH TOKEN'),
(2,'beta','REPLACE WITH TOKEN');


INSERT INTO `Channels` (`id`, `name`, `channel_id`, `channel_type`, `profanity_check_exempt`) VALUES
(8, 'setups', 0, 0, 0),
(9, 'news', 0, 0, 1),
(14, 'suggestions', 0, 0, 1),
(18, 'modlog', 0, 0, 1),
(20, 'referralcodes', 0, 0, 0),
(23, 'joinlog', 0, 0, 1),
(28, 'info', 0, 0, 1),
(39, 'warnings', 0, 0, 1),
(43, 'starboard', 0, 0, 1),
(47, 'mutes', 0, 0, 1),
(48, 'modqueue', 0, 0, 1),
(49, 'modmaillog', 0, 0, 1),
(50, 'banlog', 0, 0, 0);

INSERT INTO  `Roles` (`id` ,`name`, `role_id` ) VALUES 
(1, 'staff', 0);