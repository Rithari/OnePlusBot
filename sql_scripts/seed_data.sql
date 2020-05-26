
INSERT INTO `PersistentData` VALUES 
(1,'server_id', 0, ''),
(3,'rolemanager_message_id',0, ''),
(4,'starboard_stars',1, ''),
(5,'level_2_stars',2, ''),
(6,'level_3_stars',3, ''),
(7,'level_4_stars',3, ''),
(8,'decay_days', 90, ''),
(9, 'modmail_category_id', 0, ''),
(11, 'xp_disabled', 1, ''),
(12, 'xp_gain_range_min', 10, ''),
(13, 'xp_gain_range_max', 25, ''),
(14, 'profanity_votes_threshold', 4, ''),
(15, 'legal_user_name_regex', 0, ''),
(16, 'auto_mute_max_level', 0, ''),
(17, 'auto_mute_ping_count', 0, '');

INSERT INTO `AuthTokens` VALUES 
(1,'stable','REPLACE WITH TOKEN'),
(2,'beta','REPLACE WITH TOKEN');


INSERT INTO `Channels` (`name`, `channel_id`, `channel_type`) VALUES
('starboard', 1, 0),
('setups', 2, 0),
('referralcodes', 3, 0);

INSERT INTO  `Roles` (`name`, `role_id` ) VALUES
('staff', 0);

INSERT INTO `Emotes` (`id`, `name`, `emote_key`, `animated`, `emote_id`, `custom`) VALUES
(1, 'success', 'SUCCESS', 0, 0, 1),
(2, 'OpYes', 'OP_YES', 0, 0, 1),
(3, 'OpNo', 'OP_NO', 0, 0, 1),
(4, '🗞️', 'newspaper', 0, 0, 0),
(5, '⭐', 'STAR', 0, 0, 0),
(6, '🌟', 'LVL_2_STAR', 0, 0, 0),
(7, '💫', 'LVL_3_STAR', 0, 0, 0),
(8, 'star4', 'LVL_4_STAR', 1, 0, 1),
(9, '⚠️', 'FAIL', 0, 0, 0),
(10, '📫', 'OPEN_MODMAIL', 0, 0, 0);

INSERT INTO `ResponseTemplate` (`template_key`, `template_text`) VALUES ('ILLEGAL_NAME_MODMAIL', 'example response'), ('ILLEGAL_NAME_REMINDER_TEXT', 'Rename {0} and close.');
