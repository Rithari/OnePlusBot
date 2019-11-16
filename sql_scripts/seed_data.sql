
INSERT INTO `PersistentData` VALUES 
(1,'server_id', 0, ''),
(3,'rolemanager_message_id',0, ''),
(4,'starboard_stars',1, ''),
(5,'level_2_stars',2, ''),
(6,'level_3_stars',3, ''),
(7,'decay_days', 90, ''),
(8, 'modmail_category_id', 0, ''),
(9, 'user_name_illegal_characters', 0, ''),
(10, 'xp_disabled', 1, ''),
(11, 'xp_gain_range_min', 10, ''),
(12, 'xp_gain_range_max', 10, '');

INSERT INTO `AuthTokens` VALUES 
(1,'stable','REPLACE WITH TOKEN'),
(2,'beta','REPLACE WITH TOKEN');


INSERT INTO `Channels` (`name`, `channel_id`, `channel_type`) VALUES
('setups', 0, 0),
('referralcodes', 0, 0);

INSERT INTO  `Roles` (`name`, `role_id` ) VALUES
('staff', 0);