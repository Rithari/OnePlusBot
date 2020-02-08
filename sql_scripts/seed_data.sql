
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
(14, 'legal_user_name_regex', 0, '');

INSERT INTO `AuthTokens` VALUES 
(1,'stable','REPLACE WITH TOKEN'),
(2,'beta','REPLACE WITH TOKEN');


INSERT INTO `Channels` (`name`, `channel_id`, `channel_type`) VALUES
('setups', 0, 0),
('referralcodes', 1, 0);

INSERT INTO  `Roles` (`name`, `role_id` ) VALUES
('staff', 0);