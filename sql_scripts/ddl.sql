SET FOREIGN_KEY_CHECKS=0;

--
-- Table structure for table `AuthTokens`
--

DROP TABLE IF EXISTS `AuthTokens`;
CREATE TABLE `AuthTokens` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `type` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `token` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `Channels`
--

DROP TABLE IF EXISTS `Channels`;
CREATE TABLE `Channels` (
  `name` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `channel_id` bigint(20) unsigned NOT NULL,
  `channel_type` int(11) NOT NULL,
  PRIMARY KEY (`channel_id`)
) ENGINE=InnoDB AUTO_INCREMENT=52 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


--
-- Table structure for table `FAQCommandChannelEntry`
--

DROP TABLE IF EXISTS `FAQCommandChannelEntry`;
CREATE TABLE `FAQCommandChannelEntry` (
  `entry_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `text` mediumtext COLLATE utf8mb4_unicode_ci,
  `is_embed` tinyint(4) NOT NULL,
  `image_url` text COLLATE utf8mb4_unicode_ci,
  `hex_color` int(11) unsigned NOT NULL,
  `author` text COLLATE utf8mb4_unicode_ci,
  `author_avatar_url` text COLLATE utf8mb4_unicode_ci,
  `position` int(11) unsigned NOT NULL,
  `changed_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `command_channel_id_reference` int(10) unsigned NOT NULL,
  PRIMARY KEY (`entry_id`),
  KEY `fk_command_channel` (`command_channel_id_reference`),
  CONSTRAINT `fk_command_channel` FOREIGN KEY (`command_channel_id_reference`) REFERENCES `FaqCommandChannel` (`command_channel_id`)
) ENGINE=InnoDB AUTO_INCREMENT=359 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


--
-- Table structure for table `FaqCommand`
--

DROP TABLE IF EXISTS `FaqCommand`;
CREATE TABLE `FaqCommand` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `aliases` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=40 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `FaqCommandChannel`
--

DROP TABLE IF EXISTS `FaqCommandChannel`;
CREATE TABLE `FaqCommandChannel` (
  `command_channel_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `command_id` int(10) unsigned NOT NULL,
  `channel_group_id` int(10) unsigned NOT NULL,
  PRIMARY KEY (`command_channel_id`),
  KEY `fk_channel_group_id` (`channel_group_id`),
  KEY `command_id` (`command_id`),
  CONSTRAINT `fk_channel_group_id` FOREIGN KEY (`channel_group_id`) REFERENCES `ChannelGroups` (`id`),
  CONSTRAINT `fk_command_id` FOREIGN KEY (`command_id`) REFERENCES `FaqCommand` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=359 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `Mutes`
--

DROP TABLE IF EXISTS `Mutes`;
CREATE TABLE `Mutes` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `muted_user` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `muted_user_id` bigint(20) unsigned NOT NULL,
  `muted_by` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `muted_by_id` bigint(20) unsigned NOT NULL,
  `reason` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `mute_date` datetime NOT NULL,
  `unmute_date` datetime NOT NULL,
  `unmute_scheduled` tinyint(1) NOT NULL DEFAULT '0',
  `mute_ended` tinyint(1) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=82 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `PersistentData`
--

DROP TABLE IF EXISTS `PersistentData`;
CREATE TABLE `PersistentData` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` text NOT NULL,
  `val` bigint(20) unsigned NOT NULL,
  `string` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=latin1;


--
-- Table structure for table `User`
--

DROP TABLE IF EXISTS `User`;
CREATE TABLE `User` (
 `id` bigint(20) unsigned NOT NULL,
 `modmail_muted` tinyint(4) NOT NULL DEFAULT '0',
 `modmail_muted_until` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
 `modmail_muted_reminded` tinyint(4) NOT NULL DEFAULT '0',
 `current_level` int(10) unsigned NOT NULL DEFAULT '0',
 `xp` bigint(20) unsigned NOT NULL DEFAULT '0',
 `xp_updated` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
 `current_role_id` int(10) unsigned DEFAULT NULL,
 `message_count` bigint(20) unsigned NOT NULL DEFAULT '0',
 `xp_gain_disabled` tinyint(4) NOT NULL DEFAULT '0',
 PRIMARY KEY (`id`),
 KEY `fk_role_ref` (`current_role_id`),
 CONSTRAINT `fk_role_ref` FOREIGN KEY (`current_role_id`) REFERENCES `ExperienceRoles` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `ProfanityChecks`
--

DROP TABLE IF EXISTS `ProfanityChecks`;
CREATE TABLE `ProfanityChecks` (
 `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
 `regex` text COLLATE utf8mb4_unicode_ci NOT NULL,
 `label` text COLLATE utf8mb4_unicode_ci NOT NULL,
 PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


--
-- Table structure for table `UsedProfanity`
--

DROP TABLE IF EXISTS `UsedProfanity`;
CREATE TABLE `UsedProfanity` (
 `user_id` bigint(20) unsigned NOT NULL,
 `message_id` bigint(20) unsigned NOT NULL,
 `profanity_id` int(10) unsigned NOT NULL,
 `valid` tinyint(4) NOT NULL,
 KEY `fk_user_id` (`user_id`),
 KEY `fk_profanity_id` (`profanity_id`),
 CONSTRAINT `fk_profanity_id` FOREIGN KEY (`profanity_id`) REFERENCES `ProfanityChecks` (`id`),
 CONSTRAINT `fk_user_id` FOREIGN KEY (`user_id`) REFERENCES `User` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `ReferralCodes`
--

DROP TABLE IF EXISTS `ReferralCodes`;
CREATE TABLE `ReferralCodes` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `sender` bigint(20) unsigned NOT NULL,
  `code` text NOT NULL,
  `date` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;


--
-- Table structure for table `Roles`
--

DROP TABLE IF EXISTS `Roles`;
CREATE TABLE `Roles` (
 `name` text NOT NULL,
 `role_id` bigint(20) unsigned NOT NULL,
 `xp_role` tinyint(4) NOT NULL DEFAULT '0',
 PRIMARY KEY (`role_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Table structure for table `StarboardMessages`
--

DROP TABLE IF EXISTS `StarboardMessages`;
CREATE TABLE `StarboardMessages` (
  `message_id` bigint(20) unsigned NOT NULL,
  `starboard_message_id` bigint(20) unsigned NOT NULL,
  `star_count` int(10) unsigned NOT NULL,
  `author_id` bigint(20) unsigned NOT NULL,
  `ignored` tinyint(1) NOT NULL,
  PRIMARY KEY (`message_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `StarboardPostRelations`
--

DROP TABLE IF EXISTS `StarboardPostRelations`;
CREATE TABLE `StarboardPostRelations` (
  `user_id` bigint(20) unsigned NOT NULL,
  `message_id` bigint(20) unsigned NOT NULL,
  PRIMARY KEY (`user_id`,`message_id`),
  KEY `fk_starboardpost_id` (`message_id`),
  CONSTRAINT `fk_starboardpost_id` FOREIGN KEY (`message_id`) REFERENCES `StarboardMessages` (`message_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `Warnings`
--

DROP TABLE IF EXISTS `Warnings`;
CREATE TABLE `Warnings` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `warned_user` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `warned_user_id` bigint(20) unsigned NOT NULL,
  `warned_by` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `warned_by_id` bigint(20) unsigned NOT NULL,
  `reason` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `date` datetime NOT NULL,
  `decayed` tinyint(4) NOT NULL,
  `decayed_date` datetime DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


--
-- Table structure for table `Reminders`
--

DROP TABLE IF EXISTS `Reminders`;
CREATE TABLE `Reminders` (
 `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
 `reminded_user_id` bigint(20) unsigned NOT NULL,
 `remind_text` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
 `reminder_date` datetime NOT NULL,
 `reminder_scheduled` tinyint(1) NOT NULL DEFAULT '0',
 `reminded` tinyint(1) NOT NULL DEFAULT '1',
 `channel_id` bigint(20) unsigned NOT NULL,
 `target_date` datetime NOT NULL,
 `message_id` bigint(20) unsigned NOT NULL,
 PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=23 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `InviteLinks`
--

DROP TABLE IF EXISTS `InviteLinks`;
CREATE TABLE `InviteLinks` (
 `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
 `link` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
 `label` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
 `added_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
 PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `ModMailThread`
--

DROP TABLE IF EXISTS `ModMailThread`;
CREATE TABLE `ModMailThread` (
 `user_id` bigint(20) unsigned NOT NULL,
 `channel_id` bigint(20) unsigned NOT NULL,
 `created_date` datetime NOT NULL,
 `update_date` datetime NOT NULL,
 `closed_date` datetime NOT NULL,
 `state` varchar(25) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'INITIAL',
 PRIMARY KEY (`channel_id`),
 KEY `fk_user_id_ref` (`user_id`),
 CONSTRAINT `fk_user_id_ref` FOREIGN KEY (`user_id`) REFERENCES `User` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `ThreadSubscribers`
--

DROP TABLE IF EXISTS `ThreadSubscribers`;
CREATE TABLE `ThreadSubscribers` (
 `channel_id` bigint(20) unsigned NOT NULL,
 `user_id` bigint(20) unsigned NOT NULL,
 PRIMARY KEY (`channel_id`,`user_id`),
 CONSTRAINT `fk_thread_ref` FOREIGN KEY (`channel_id`) REFERENCES `ModMailThread` (`channel_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


--
-- Table structure for table `ThreadMessage`
--

DROP TABLE IF EXISTS `ThreadMessage`;
CREATE TABLE `ThreadMessage` (
 `channel_id` bigint(20) unsigned NOT NULL,
 `channel_message_id` bigint(20) unsigned NOT NULL,
 `user_message_id` bigint(20) unsigned NOT NULL,
 `user_id` bigint(20) unsigned NOT NULL,
 `anonymous` tinyint(4) NOT NULL,
 PRIMARY KEY (`channel_id`,`channel_message_id`),
 CONSTRAINT `fk_msg_id` FOREIGN KEY (`channel_id`) REFERENCES `ModMailThread` (`channel_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `ChannelGroups`
--

DROP TABLE IF EXISTS `ChannelGroups`;
CREATE TABLE `ChannelGroups` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
 `name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `profanity_check_exempt` tinyint(4) NOT NULL,
  `invite_check_exempt` tinyint(4) NOT NULL,
  `exp_gain_exempt` tinyint(4) NOT NULL,
  `disabled` tinyint(4) NOT NULL,
 PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `ChannelInGroup`
--

DROP TABLE IF EXISTS `ChannelInGroup`;
CREATE TABLE `ChannelInGroup` (
 `channel_id` bigint(20) unsigned NOT NULL,
 `channel_group_id` int(10) unsigned NOT NULL,
 PRIMARY KEY (`channel_id`,`channel_group_id`),
 KEY `fk_group_id` (`channel_group_id`),
 CONSTRAINT `fk_group_id` FOREIGN KEY (`channel_group_id`) REFERENCES `ChannelGroups` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `PostTargets`
--

DROP TABLE IF EXISTS `PostTargets`;
CREATE TABLE `PostTargets` (
 `channel_id` bigint(20) unsigned NOT NULL,
 `name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
 PRIMARY KEY (`name`),
 UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


--
-- Table structure for table `ExperienceRoles`
--

DROP TABLE IF EXISTS `ExperienceRoles`;
CREATE TABLE `ExperienceRoles` (
 `role_id` bigint(20) unsigned NOT NULL,
 `level` int(10) unsigned NOT NULL,
 `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
 PRIMARY KEY (`id`),
 KEY `fk_level` (`level`),
 KEY `fk_role` (`role_id`),
 CONSTRAINT `fk_level` FOREIGN KEY (`level`) REFERENCES `ExperienceLevels` (`level`),
 CONSTRAINT `fk_role` FOREIGN KEY (`role_id`) REFERENCES `Roles` (`role_id`)
) ENGINE=InnoDB AUTO_INCREMENT=41 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `ExperienceRoles`
--

DROP TABLE IF EXISTS `ExperienceLevels`;
CREATE TABLE `ExperienceLevels` (
 `level` int(10) unsigned NOT NULL,
 `needed_experience` bigint(20) unsigned NOT NULL,
 PRIMARY KEY (`level`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

SET FOREIGN_KEY_CHECKS=1;